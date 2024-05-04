using Clutch.API.Models.Image;

// If we want caching, we can add the IRealProvider interface
namespace Clutch.API.Providers.Interfaces
{
    public interface IRegistryProvider
    {
        Task<RegistryManifest> GetManifestAsync(ContainerImageRequest request);
        Task<bool> SetManifestAsync(RegistryManifest manifest);
        Task<bool> DeleteManifestAsync(ContainerImageRequest request);
        
        // Revisit this method
        Task<IEnumerable<ContainerImageModel>?> GetLatestManifestsAsync();
    }
}
