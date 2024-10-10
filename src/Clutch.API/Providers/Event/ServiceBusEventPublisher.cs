using Polly;
using Polly.Wrap;
using Azure.Messaging.ServiceBus;
using DataFerry.Properties;

// TODO:
// - Unit Tests
// This class will publish events to Azure Service Bus
// Github Actions Workflow will subscribe to those events
// and perform the needed actions (ex building images)
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
        private readonly AsyncPolicyWrap<object> _policy;

        public ServiceBusEventPublisher(ServiceBusClient client, string queueName, string dlqName, IOptions<EventPublisherSettings> settings, IOptions<CacheSettings> cacheSettings, ILogger logger)
        {
            ArgumentNullException.ThrowIfNull(client);
            ArgumentException.ThrowIfNullOrEmpty(queueName);
            //ArgumentNullException.ThrowIfNullOrEmpty(dlqName);
            ArgumentNullException.ThrowIfNull(settings);
            ArgumentNullException.ThrowIfNull(logger);

            _client = client;
            _queueName = queueName;
            _dlqName = dlqName;
            _settings = settings.Value;
            _logger = logger;
            _messageTimeToLive = TimeSpan.FromHours(_settings.TimeToLive);
            _policy = PollyPolicyGenerator.GeneratePolicy(logger, cacheSettings.Value);
        }

        public async Task<bool> PublishEventAsync(string eventName, ContainerImageModel image)
        {
            await using var sender = _client.CreateSender(_queueName);

            var message = new ServiceBusMessage(
                JsonSerializer.Serialize(new
                {
                    EventData = image
                }))
            {
                TimeToLive = _messageTimeToLive
            };

            if (!string.IsNullOrEmpty(eventName))
            {
                message.ApplicationProperties.Add("EventName", eventName);
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
                }, new Context("ServiceBus.PublishEventAsync") { { "EventName", eventName } });

                return result is HttpResponseMessage http
                    ? http.IsSuccessStatusCode
                    : default;
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
                            var eventData = JsonSerializer.Deserialize<BuildEvent>(messageBody);

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
        private static bool IsTransientError(string deadLetterReason)
        {
            // These are mostly placeholder patterns and will need
            // to be verified after the Service Bus is provisioned
            var transientReasons = new[] { "Timeout", "ServerBusy", "ConnectionLost" };
            return transientReasons.Contains(deadLetterReason);
        }
    }
}