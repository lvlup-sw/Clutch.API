namespace Clutch.API.Models.Image
{
    public class ContainerImageResponseData(bool success, RegistryManifest properties, ContainerImageModel image)
    {
        public bool Success { get; set; } = success;
        public RegistryManifest RegistryManifest { get; set; } = properties;
        public ContainerImageModel ContainerImageModel { get; set; } = image;
    }
}