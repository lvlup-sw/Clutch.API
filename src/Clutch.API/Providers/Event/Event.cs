namespace Clutch.API.Providers.Event
{
    public class Event
    {
        public required string EventName { get; set; }
        public required ContainerImageModel EventData { get; set; }
    }
}
