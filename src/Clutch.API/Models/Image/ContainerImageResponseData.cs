namespace Clutch.API.Models.Image
{
    public class ContainerImageResponseData(bool success, RegistryProperties properties, ContainerImageModel image)
    {
        public bool Success { get; set; } = success;
        public RegistryProperties RegistryProperties { get; set; } = properties;
        public ContainerImageModel ContainerImageModel { get; set; } = image;
    }
}