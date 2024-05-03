namespace Clutch.API.Models.Image
{
    public class ContainerImageResponseData(bool success, ContainerImageModel image, RegistryManifest manifest)
    {
        public bool Success { get; set; } = success;
        public ContainerImageModel ContainerImageModel { get; set; } = image;
        public RegistryManifest RegistryManifest { get; set; } = manifest;
    }
}