namespace Clutch.API.Models.Image
{
    public class ContainerImageRequest
    {
        public required string Repository { get; set; }

        public required string Tag { get; set; }

        public required RegistryType RegistryType { get; set; }
    }
}
