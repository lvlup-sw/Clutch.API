using CacheProvider.Providers.Interfaces;
using Clutch.API.Models.Image;

namespace Clutch.API.Providers.Interfaces
{
    public class RegistryProviderBase : IRegistryProvider
    {
        public Task<RegistryManifest> GetManifestAsync(ContainerImageRequest request, string version)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetManifestAsync(RegistryManifest manifest)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteManifestAsync(ContainerImageRequest request, string version)
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