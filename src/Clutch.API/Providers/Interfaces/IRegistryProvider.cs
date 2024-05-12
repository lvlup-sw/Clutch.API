using Clutch.API.Models.Image;
using Clutch.API.Models.Registry;

// If we want caching, we can add the IRealProvider interface
namespace Clutch.API.Providers.Interfaces
{
    public interface IRegistryProvider : IDisposable
    {
        Task<RegistryManifestModel> GetManifestAsync(ContainerImageRequest request);
    }
}
