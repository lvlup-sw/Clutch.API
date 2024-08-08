using Azure.Messaging.ServiceBus;

// TODO:
// - DI
// - PublishEventAsync
// - Unit Tests
namespace Clutch.API.Providers.Event
{
    public class ServiceBusEventPublisher : IEventPublisher
    {
        private readonly ServiceBusClient _client;
        private readonly string _queueOrTopicName; // Queue or topic to publish to

        public ServiceBusEventPublisher(string connectionString, string queueOrTopicName)
        {
            _client = new ServiceBusClient(connectionString);
            _queueOrTopicName = queueOrTopicName;
        }

        public async Task<bool> PublishEventAsync(string eventMessage, ContainerImageModel image)
        {
            await using var sender = _client.CreateSender(_queueOrTopicName);

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
}
