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

        private AsyncPolicyWrap<HttpResponseMessage> CreatePolicy() // Adjust return type if needed
        {
            // Retry Policy
            var retryPolicy = Policy<HttpResponseMessage>
                .Handle<ServiceBusException>(ex => ex.Reason == ServiceBusFailureReason.ServiceCommunicationProblem)
                .OrResult(r => !r.IsSuccessStatusCode) // Retry on non-success HTTP status codes from Azure Service Bus
                .WaitAndRetryAsync(
                    retryCount: _settings.RetryCount, 
                    sleepDurationProvider: retryAttempt => _settings.UseExponentialBackoff 
                        ? TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) 
                        : TimeSpan.FromSeconds(_settings.RetryInterval) + TimeSpan.FromMilliseconds(new Random().Next(0, 100)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        _logger.LogInformation($"Retry {retryCount} of {_settings.RetryCount} after {timespan.TotalSeconds} seconds due to: {outcome.Exception?.Message ?? outcome.Result.ReasonPhrase}");
                    });

            // Circuit Breaker Policy
            var circuitBreakerPolicy = Policy<HttpResponseMessage>
                .Handle<ServiceBusException>()
                .OrResult(r => !r.IsSuccessStatusCode) 
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: 5, 
                    durationOfBreak: TimeSpan.FromMinutes(1),
                    onBreak: (outcome, breakDelay) =>
                    {
                        _logger.LogError($"Circuit broken for {breakDelay.TotalSeconds} seconds due to: {outcome.Exception?.Message ?? outcome.Result.ReasonPhrase}");
                    },
                    onReset: () => _logger.LogInformation("Circuit Reset"),
                    onHalfOpen: () => _logger.LogInformation("Circuit Half-Open"));

            // Fallback Policy (Optional, but recommended)
            var fallbackPolicy = Policy<HttpResponseMessage>
                .Handle<BrokenCircuitException>()
                .FallbackAsync(
                    fallbackAction: _ => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable)), // Or a default response
                    onFallbackAsync: (outcome, context) =>
                    {
                        _logger.LogError("Fallback executed due to broken circuit.");
                        return Task.CompletedTask;
                    });

            // Combine policies (order matters!)
            return fallbackPolicy.WrapAsync(circuitBreakerPolicy.WrapAsync(retryPolicy));
        }
    }
}
