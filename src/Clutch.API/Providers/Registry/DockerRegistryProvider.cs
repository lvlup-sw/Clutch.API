using Clutch.API.Models.Image;
using Clutch.API.Models.Registry;
using Clutch.API.Properties;
using Clutch.API.Providers.Interfaces;
using Microsoft.Extensions.Options;

namespace Clutch.API.Providers.Registry
{
    public class DockerRegistryProvider(ILogger logger, IOptions<AppSettings> settings) : RegistryProviderBase(logger, settings)
    {
        private readonly ILogger _logger = logger;
        private readonly AppSettings _settings = settings.Value;
        // private readonly RestClient _restClient = new("https://ghcr.io");

        public override async Task<RegistryManifestModel> GetManifestAsync(ContainerImageRequest request)
        {

        }

        public override async Task<bool> SetManifestAsync(ContainerImageRequest request)
        {
            // We will trigger the build pipeline here

            return true;
        }

        public override async Task<bool> DeleteManifestAsync(ContainerImageRequest request)
        {
            // We will call the api here

            return true;
        }

        public override async Task<IEnumerable<ContainerImageModel>?> GetLatestManifestsAsync()
        {
            throw new NotImplementedException();
        }
    }
}
