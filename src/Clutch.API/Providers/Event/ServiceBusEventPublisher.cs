using Polly;
using Polly.Wrap;
using Polly.CircuitBreaker;
using Azure.Messaging.ServiceBus;

// TODO:
// - DI
// - PublishEventAsync
// - Unit Tests
// This class will publish events to Azure Service Bus
// Github Actions Workflow will subscribe to those events
// and perform the needed actions
namespace Clutch.API.Providers.Event
{
    public class ServiceBusEventPublisher : IEventPublisher
    {
        private readonly ServiceBusClient _client;
        private readonly string _queueName;
        private readonly string _dlqName;
        private readonly EventPublisherSettings _settings;
        private readonly ILogger _logger;
        private readonly TimeSpan _messageTimeToLive;
        private readonly AsyncPolicyWrap<HttpResponseMessage> _policy;

        public ServiceBusEventPublisher(ServiceBusClient client, string queueName, string dlqName, IOptions<EventPublisherSettings> settings, ILogger logger)
        {
            ArgumentNullException.ThrowIfNull(client);
            ArgumentNullException.ThrowIfNullOrEmpty(queueName);
            ArgumentNullException.ThrowIfNullOrEmpty(dlqName);
            ArgumentNullException.ThrowIfNull(settings);
            ArgumentNullException.ThrowIfNull(logger);

            _client = client;
            _queueName = queueName;
            _dlqName = dlqName;
            _settings = settings.Value;
            _logger = logger;
            _messageTimeToLive = TimeSpan.FromHours(24);
            _policy = CreatePolicy();
        }

        public async Task<bool> PublishEventAsync(string eventName, ContainerImageModel image)
        {
            await using var sender = _client.CreateSender(_queueName);

            var message = new ServiceBusMessage(
                JsonSerializer.Serialize(new
                {
                    EventName = eventName,
                    EventData = image
                }))
            {
                TimeToLive = _messageTimeToLive
            };

            if (!string.IsNullOrEmpty(image.Repository))
            {
                message.ApplicationProperties.Add("Repository", image.Repository);
            }

            // Execute the API operation
            try
            {
                var result = await _policy.ExecuteAsync(async (ctx) =>
                {
                    _logger.LogDebug("Attempting to publish event '{eventName}' to Service Bus.", eventName);

                    try
                    {
                        await sender.SendMessageAsync(message);
                        // Note that since this is a fire and forget operation
                        // we return an 200 status code as long as there were
                        // no exceptions during the call
                        return new HttpResponseMessage(HttpStatusCode.OK);
                    }
                    catch (Exception innerEx)
                    {
                        _logger.LogError(innerEx.GetBaseException(), "Error sending message to Service Bus.");
                        // Re-throw to let Polly handle retries/fallback
                        throw;
                    }
                }, new Context("ServiceBus.PublishEventAsync"){{ "EventName", eventName }});

                return result.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish event after retries and fallback.");
                return false;
            }
        }

        // Currently we discard events that did not occur due to Transient errors.
        // In the future, we may want to instead upload those problematic events
        // to a database/storage for further analysis. Additional refinements
        // could involve introducing more logging/metrics for observability.
        public async Task ProcessDeadLetterQueueAsync()
        {
            await using var receiver = _client.CreateReceiver(_dlqName, new ServiceBusReceiverOptions { ReceiveMode = ServiceBusReceiveMode.PeekLock });

            while (true)
            {
                ServiceBusReceivedMessage receivedMessage = await receiver.ReceiveMessageAsync();

                if (receivedMessage is not null)
                {
                    try
                    {
                        // Deserialize and Log the Message
                        var messageBody = receivedMessage.Body.ToString();
                        var deadLetterReason = receivedMessage.DeadLetterReason;
                        var deadLetterErrorDescription = receivedMessage.DeadLetterErrorDescription;

                        _logger.LogWarning("Processing dead-lettered message. Reason: {reason}, Description: {description}, Body: {body}",
                            deadLetterReason, deadLetterErrorDescription, messageBody);

                        // Check for Transient Error and Retry
                        if (IsTransientError(deadLetterReason))
                        {
                            var eventData = JsonSerializer.Deserialize<Event>(messageBody);

                            if (eventData is null)
                            {
                                _logger.LogError("Failed to deserialize dead-lettered message. Body: {body}", messageBody);
                                continue;
                            }

                            // Retry with exponential backoff
                            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, receivedMessage.DeliveryCount)));

                            var success = await PublishEventAsync(eventData.EventName, eventData.EventData);
                            if (success)
                            {
                                _logger.LogInformation("Successfully re-published dead-lettered event '{eventName}'.", eventData.EventName);
                            }
                            else
                            {
                                _logger.LogError("Failed to re-publish dead-lettered event '{eventName}'.", eventData.EventName);
                                // Consider additional handling or storage for persistent failures after retries
                            }
                        }
                        else
                        {
                            _logger.LogError("Discarding dead-lettered message with non-transient failure. Reason: {reason}, Description: {description}",
                                deadLetterReason, deadLetterErrorDescription);
                        }

                        // Complete the Message (Remove from DLQ)
                        await receiver.CompleteMessageAsync(receivedMessage);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing dead-lettered message. Message will be abandoned.");
                        await receiver.AbandonMessageAsync(receivedMessage);
                    }
                }
                else
                {
                    // No more messages, introduce a delay before checking again
                    await Task.Delay(TimeSpan.FromSeconds(10));
                }
            }
        }

        // Helper method to identify transient errors
        private bool IsTransientError(string deadLetterReason)
        {
            // These are mostly placeholder patterns and will need
            // to be verified after the Service Bus is provisioned
            var transientReasons = new[] { "Timeout", "ServerBusy", "ConnectionLost" };
            return transientReasons.Contains(deadLetterReason);
        }

        // Polly retry, circuitbreaker, and fallback for resiliency
        private AsyncPolicyWrap<HttpResponseMessage> CreatePolicy()
        {
            // Retry Policy
            var retryPolicy = Policy<HttpResponseMessage>
                .Handle<ServiceBusException>(ex => ex.Reason is ServiceBusFailureReason.ServiceCommunicationProblem)
                .OrResult(r => !r.IsSuccessStatusCode) // Retry on non-success HTTP status codes from Azure Service Bus
                .WaitAndRetryAsync(
                    retryCount: _settings.RetryCount,
                    // Exponential backoff or fixed interval with jitter
                    sleepDurationProvider: retryAttempt => _settings.UseExponentialBackoff
                        ? TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                        : TimeSpan.FromSeconds(_settings.RetryInterval)
                            + TimeSpan.FromMilliseconds(new Random().Next(0, 100)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        _logger.LogInformation($"Retry {retryCount} of {_settings.RetryCount} after {timespan.TotalSeconds} seconds due to: {outcome.Exception?.Message ?? outcome.Result.ReasonPhrase}");
                    });

            // Circuit Breaker Policy with Exponential Backoff
            var circuitBreakerPolicy = Policy
                .Handle<ServiceBusException>()
                .Or<TimeoutException>()
                .Or<TaskCanceledException>()
                .AdvancedCircuitBreakerAsync(
                    failureThreshold: 0.5, // 50% failure rate
                    samplingDuration: TimeSpan.FromSeconds(30), // Evaluate failures over 30 seconds
                    minimumThroughput: 10, // Minimum 10 calls before tripping
                    durationOfBreak: TimeSpan.FromMinutes(1),
                    onBreak: (outcome, breakDelay) =>
                    {
                        try
                        {
                            _logger.LogError(
                                "Circuit breaker opened for {breakDelay} seconds due to: {exceptionMessage}",
                                breakDelay.TotalSeconds,
                                outcome.Message
                            );
                        }
                        catch (Exception unexpectedException)
                        {
                            _logger.LogError(unexpectedException, "An error occurred during circuit breaker 'onBreak' handling.");
                        }
                    },
                    onReset: () => _logger.LogInformation("Circuit Reset"),
                    onHalfOpen: () => _logger.LogInformation("Circuit Half-Open"));

            // Fallback Policy
            var fallbackPolicy = Policy
                .Handle<BrokenCircuitException>()
                .FallbackAsync(
                    fallbackAction: _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)),
                    onFallbackAsync: (outcome) =>
                    {
                        _logger.LogError("Fallback executed due to broken circuit.");
                        return Task.CompletedTask;
                    });

            return fallbackPolicy.WrapAsync(circuitBreakerPolicy.WrapAsync(retryPolicy));
        }
    }
}