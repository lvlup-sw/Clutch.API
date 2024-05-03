namespace Clutch.API.Models.Image
{
    public class ContainerImageResponse(bool success, ContainerImageVersion version, RegistryManifest manifest)
    {
        public bool Success { get; set; } = success;
        public ContainerImageVersion ContainerImageVersion { get; set; } = version;
        public RegistryManifest RegistryManifest { get; set; } = manifest;
    }
}