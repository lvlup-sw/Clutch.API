using Clutch.API.Models.Enums;
using Clutch.API.Properties;
using Clutch.API.Providers.Interfaces;
using Microsoft.Extensions.Options;

namespace Clutch.API.Providers.Registry
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
                true => ActivatorUtilities.CreateInstance(_serviceProvider, provider, GetLogger(provider), GetSettings()),
                false => ActivatorUtilities.CreateInstance(_serviceProvider, typeof(RegistryProviderBase), GetLogger(typeof(RegistryProviderBase)), GetSettings())
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

        private ILogger GetLogger(Type provider) => _serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(provider.Name);

        private IOptions<AppSettings> GetSettings() => _serviceProvider.GetRequiredService<IOptions<AppSettings>>();
    }
}