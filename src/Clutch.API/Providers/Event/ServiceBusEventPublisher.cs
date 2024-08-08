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
    public class ServiceBusEventPublisher(ServiceBusClient client, string queueName) : IEventPublisher
    {
        private readonly ServiceBusClient _client = client;
        private readonly string _queueName = queueName;

        public async Task<bool> PublishEventAsync(string eventMessage, ContainerImageModel image)
        {
            await using var sender = _client.CreateSender(_queueName);

            var messageBody = JsonSerializer.Serialize(new
            {
                EventMessage = eventMessage,
                EventData = image
            });

            var message = new ServiceBusMessage(messageBody);
            await sender.SendMessageAsync(message);
            return true;
        }
    }

    /* DLQ and Polly impl
    using Azure.Messaging.ServiceBus;
    using Polly;
    using Polly.Retry;
    using System.Text.Json;

    public class ServiceBusEventPublisher : IEventPublisher
    {
        private readonly ServiceBusClient _client;
        private readonly string _queueOrTopicName;
        private readonly string _deadLetterQueueName; // Explicitly define the DLQ name
        private readonly int _maxDeliveryCount = 5; 
        private readonly TimeSpan _messageTimeToLive = TimeSpan.FromHours(24); 

        // Polly retry policy
        private readonly AsyncRetryPolicy _retryPolicy = Policy
            .Handle<ServiceBusException>(ex => ex.Reason == ServiceBusFailureReason.ServiceCommunicationProblem)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        public ServiceBusEventPublisher(
            ServiceBusClient client, 
            string queueOrTopicName, 
            string deadLetterQueueName, // Add this parameter
            int maxDeliveryCount = 5, 
            TimeSpan? messageTimeToLive = null)
        {
            _client = client;
            _queueOrTopicName = queueOrTopicName;
            _deadLetterQueueName = deadLetterQueueName; // Initialize
            _maxDeliveryCount = maxDeliveryCount;
            _messageTimeToLive = messageTimeToLive ?? TimeSpan.FromHours(24);
        }

        public async Task PublishAsync(string eventName, object eventData)
        {
            await using var sender = _client.CreateSender(_queueOrTopicName);

            var message = new ServiceBusMessage(
                JsonSerializer.Serialize(new { EventName = eventName, EventData = eventData })
            )
            {
                MaxDeliveryCount = _maxDeliveryCount,
                TimeToLive = _messageTimeToLive
            };

            try
            {
                await _retryPolicy.ExecuteAsync(async () => await sender.SendMessageAsync(message));
            }
            catch (ServiceBusException ex)
            {
                // Log or handle the exception (Polly has already retried)
            }
        }
    
        public async Task ProcessDeadLetterQueueAsync()
        {
            await using var receiver = _client.CreateReceiver(_deadLetterQueueName);

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
    }
     
    */ 
}
