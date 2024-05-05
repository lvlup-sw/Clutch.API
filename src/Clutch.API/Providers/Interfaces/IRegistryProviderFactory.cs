using Clutch.API.Models.Image;

// If we want caching, we can add the IRealProvider interface
namespace Clutch.API.Providers.Interfaces
{
    public interface IRegistryProviderFactory
    {
        IRegistryProvider CreateRegistryProvider(RegistryType type);
    }
}