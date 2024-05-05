namespace Clutch.API.Models.Image
{
    public class ContainerImageResponse(bool success, ContainerImage version, RegistryManifest manifest)
    {
        public bool Success { get; set; } = success;
        public ContainerImage ContainerImage { get; set; } = version;
        public RegistryManifest RegistryManifest { get; set; } = manifest;
    }
}