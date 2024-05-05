using Clutch.API.Models.Image;
using Clutch.API.Models.Registry;

// If we want caching, we can add the IRealProvider interface
namespace Clutch.API.Providers.Interfaces
{
    public interface IRegistryProvider
    {
        Task<RegistryManifestModel> GetManifestAsync(ContainerImageRequest request);
        Task<bool> SetManifestAsync(ContainerImageRequest request);
        Task<bool> DeleteManifestAsync(ContainerImageRequest request);
        
        // Revisit this method
        Task<IEnumerable<ContainerImageModel>?> GetLatestManifestsAsync();
    }
}
