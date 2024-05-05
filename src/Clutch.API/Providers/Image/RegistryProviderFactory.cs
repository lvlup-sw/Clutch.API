using Clutch.API.Models.Image;
using Clutch.API.Providers.Interfaces;

namespace Clutch.API.Providers.Image
{
    public class RegistryProviderFactory(IServiceProvider serviceProvider) : IRegistryProviderFactory
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly Dictionary<RegistryType, Type> _registryProviderMap = CreateMappings();

        public IRegistryProvider CreateRegistryProvider(RegistryType type)
        {
            bool success = _registryProviderMap.TryGetValue(type, out Type? provider);

            object? instance = (success && provider is not null) switch
            {
                true  => Activator.CreateInstance(provider, _serviceProvider),
                false => Activator.CreateInstance(typeof(RegistryProviderBase), _serviceProvider)
            };

            // This is kinda hacky but the compiler is being dumb
            return instance as IRegistryProvider ?? default!;
        }

        // Update mappings as new implementations are introduced
        private static Dictionary<RegistryType, Type> CreateMappings()
        {
            return new()
            {
                { RegistryType.Local, typeof(RegistryProviderBase) },
                { RegistryType.Docker, typeof(RegistryProviderBase) },
                { RegistryType.Google, typeof(RegistryProviderBase) },
                { RegistryType.AWS, typeof(RegistryProviderBase) },
                { RegistryType.Azure, typeof(RegistryProviderBase) },
                { RegistryType.Harbor, typeof(RegistryProviderBase) },
                { RegistryType.Artifactory, typeof(RegistryProviderBase) }
            };
        }
    }
}