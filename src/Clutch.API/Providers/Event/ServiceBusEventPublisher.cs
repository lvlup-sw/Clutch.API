using Azure.Messaging.ServiceBus;
using Polly;
using Polly.Retry;
using System.Text.Json;

// TODO:
// - DI
// - PublishEventAsync
// - Unit Tests
// This class will publish events to Azure Service Bus
// Github Actions Workflow will subscribe to those events
// and perform the needed actions
namespace Clutch.API.Providers.Event
{
    public class ServiceBusEventPublisher(ServiceBusClient client, string queueName, string dlqName) : IEventPublisher
    {
        private readonly ServiceBusClient _client = client;
        private readonly string _queueName = queueName;
        private readonly string _dlqName = dlqName;
        private readonly TimeSpan _messageTimeToLive = TimeSpan.FromHours(24);

        // Polly retry policy
        private readonly AsyncRetryPolicy _retryPolicy = Policy
            .Handle<ServiceBusException>(ex => ex.Reason == ServiceBusFailureReason.ServiceCommunicationProblem)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        public async Task<bool> PublishEventAsync(string eventName, ContainerImageModel image)
        {
            await using var sender = _client.CreateSender(_queueName);

            var message = new ServiceBusMessage(JsonSerializer.Serialize(new { EventName = eventName, EventData = image }))
            {
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
    }
}
