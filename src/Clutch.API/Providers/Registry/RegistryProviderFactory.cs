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
                true  => ActivatorUtilities.CreateInstance(_serviceProvider, provider, GetLogger(provider), GetSettings()),
                false => ActivatorUtilities.CreateInstance(_serviceProvider, typeof(RegistryProviderBase), GetLogger(typeof(RegistryProviderBase)), GetSettings())
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

        private IOptions<AppSettings> GetSettings() => _serviceProvider.GetRequiredService<IOptions<AppSettings>>();
    }
}