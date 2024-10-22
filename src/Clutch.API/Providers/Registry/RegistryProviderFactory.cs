using System.Buffers;

namespace Clutch.API.Providers.Registry
{
    public class RegistryProviderFactory(IServiceProvider serviceProvider) : IRegistryProviderFactory
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly Dictionary<RegistryType, Type> _registryProviderMap = CreateMappings();

        public IRegistryProvider? CreateRegistryProvider(RegistryType type)
        {
            bool success = _registryProviderMap.TryGetValue(type, out Type? provider);

            object instance = (success && provider is not null) switch
            {
                true  => ActivatorUtilities.CreateInstance(_serviceProvider, provider, GetArrayPool(), GetLogger(provider), GetConfiguration()),
                false => ActivatorUtilities.CreateInstance(_serviceProvider, typeof(RegistryProviderBase), GetArrayPool(), GetLogger(typeof(RegistryProviderBase)), GetConfiguration())
            };

            return instance as IRegistryProvider;
        }

        // Update mappings as new implementations are introduced
        private static Dictionary<RegistryType, Type> CreateMappings()
        {
            return new()
            {
                { RegistryType.Local, typeof(RegistryProviderBase) },
                { RegistryType.Docker, typeof(DockerRegistryProvider) },
                { RegistryType.Google, typeof(GoogleRegistryProvider) },
                { RegistryType.AWS, typeof(AWSRegistryProvider) },
                { RegistryType.Azure, typeof(AzureRegistryProvider) },
                { RegistryType.Artifactory, typeof(ArtifactoryRegistryProvider) }
            };
        }

        private ILogger GetLogger(Type provider) => _serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(provider.Name);

        private IConfiguration GetConfiguration() => _serviceProvider.GetRequiredService<IConfiguration>();

        private ArrayPool<byte> GetArrayPool() => _serviceProvider.GetRequiredService<ArrayPool<byte>>();
    }
}