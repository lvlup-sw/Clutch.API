namespace Clutch.API.Models.Image
{
    public class ContainerImageBatchRequest
    {
        public required IEnumerable<ContainerImageRequest> Requests { get; set; }
    }
}
