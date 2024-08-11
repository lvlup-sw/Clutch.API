namespace Clutch.API.Providers.Interfaces
{
    // If we want to use another event subscriber
    // we can move to a Factory pattern like we
    // for the Registry Providers
    public interface IEventPublisher
    {
        Task<bool> PublishEventAsync(string eventMessage, ContainerImageModel image);
    }
}
