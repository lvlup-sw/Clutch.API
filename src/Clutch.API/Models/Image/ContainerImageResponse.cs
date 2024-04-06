namespace Clutch.API.Models.Image
{
    public class ContainerImageResponse(bool success, RegistryManifest properties, ContainerImage image)
    {
        public bool Success { get; set; } = success;
        public RegistryManifest RegistryManifest { get; set; } = properties;
        public ContainerImage ContainerImage { get; set; } = image;
    }
}