namespace Clutch.API.Models.Event
{
    public class BuildEvent
    {
        public required string EventName { get; set; }
        public required ContainerImageModel EventData { get; set; }
    }
}
