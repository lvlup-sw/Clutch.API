using RestSharp;

namespace Clutch.API.Providers.Registry
{
    public class AWSRegistryProvider(IRestClientFactory restClientFactory, ILogger logger, IOptions<AppSettings> settings) : RegistryProviderBase(restClientFactory, logger, settings)
    {
        public override async Task<RegistryManifestModel> GetManifestAsync(ContainerImageRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
