namespace StepNet.API.Models.Image
{
    public class ContainerImageResponse(bool success, RegistryProperties properties, ContainerImage image)
    {
        public bool Success { get; set; } = success;
        public RegistryProperties RegistryProperties { get; set; } = properties;
        public ContainerImage ContainerImage { get; set; } = image;
    }
}