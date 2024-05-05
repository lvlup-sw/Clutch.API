using Clutch.API.Models.Registry;

namespace Clutch.API.Models.Image
{
    public class ContainerImageBuildResult
    {
        public bool Success { get; set; }

        public required RegistryManifestModel RegistryManifestModel { get; set; }
    }
}
