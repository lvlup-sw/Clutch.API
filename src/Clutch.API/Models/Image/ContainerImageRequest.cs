using Clutch.API.Models.Enums;

namespace Clutch.API.Models.Image
{
    public record ContainerImageRequest
    {
        public required string Repository { get; init; }

        public required string Tag { get; init; }

        public required RegistryType RegistryType { get; init; }
    }
}
