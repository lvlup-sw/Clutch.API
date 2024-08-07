using RestSharp;

namespace Clutch.API.Providers.Registry
{
    public class GoogleRegistryProvider(IRestClientFactory restClientFactory, ILogger logger, IConfiguration configuration) : RegistryProviderBase(restClientFactory, logger, configuration)
    {
        public override async Task<RegistryManifestModel> GetManifestAsync(ContainerImageRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
