﻿using Polly;
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
        private readonly AsyncPolicyWrap<HttpResponseMessage> _retryPolicy;

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
            _retryPolicy = CreatePolicy();
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

            try
            {
                //await _retryPolicy.ExecuteAsync(async () => await sender.SendMessageAsync(message));
            }
            catch (ServiceBusException ex)
            {
                // Log or handle the exception (Polly has already retried)
            }



            return true;
        }

        public async Task ProcessDeadLetterQueueAsync()
        {
            await using var receiver = _client.CreateReceiver(_dlqName);

            while (true)
            {
                ServiceBusReceivedMessage receivedMessage = await receiver.ReceiveMessageAsync();

                if (receivedMessage != null)
                {
                    // Process the dead-lettered message
                    // ... (your logic here)
                    await receiver.CompleteMessageAsync(receivedMessage); // Remove from DLQ after processing
                }
                else
                {
                    // Optional: Wait for a period before checking again (e.g., Task.Delay(TimeSpan.FromSeconds(10)))
                    break; // No more messages in the DLQ
                }
            }
        }

        // Polly retry, circuitbreaker, and fallback for resiliency
        private AsyncPolicyWrap<HttpResponseMessage> CreatePolicy()
        {
            // Retry Policy
            var retryPolicy = Policy<HttpResponseMessage>
                .Handle<ServiceBusException>(ex => ex.Reason == ServiceBusFailureReason.ServiceCommunicationProblem)
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