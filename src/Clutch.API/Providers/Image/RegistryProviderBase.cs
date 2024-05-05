using CacheProvider.Providers.Interfaces;
using Clutch.API.Models.Image;
using Clutch.API.Properties;
using Microsoft.Extensions.Options;

namespace Clutch.API.Providers.Interfaces
{
    // Implements GHCR
    public class RegistryProviderBase(ILogger logger, IOptions<AppSettings> settings) : IRegistryProvider
    {
        private readonly ILogger _logger = logger;
        private readonly AppSettings _settings = settings.Value;

        public Task<RegistryManifestModel> GetManifestAsync(ContainerImageRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetManifestAsync(RegistryManifestModel manifest)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteManifestAsync(ContainerImageRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ContainerImageModel>?> GetLatestManifestsAsync()
        {
            throw new NotImplementedException();
        }

        // We can include protected helper methods here for common operations
    }
}