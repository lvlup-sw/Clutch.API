namespace Clutch.API.Models.Image
{
    public class ContainerImageResponseData(bool success, ContainerImageModel image, RegistryManifestModel manifest)
    {
        public bool Success { get; set; } = success;
        public ContainerImageModel ContainerImageModel { get; set; } = image;
        public RegistryManifestModel RegistryManifestModel { get; set; } = manifest;
    }
}