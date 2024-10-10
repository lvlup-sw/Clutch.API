using Clutch.API.Models.Registry;

namespace Clutch.API.Models.Image
{
    public record ContainerImageResponse(bool success, ContainerImage version, RegistryManifest manifest)
    {
        public bool Success { get; init; } = success;
        public ContainerImage ContainerImage { get; init; } = version;
        public RegistryManifest RegistryManifest { get; init; } = manifest;
    }
}