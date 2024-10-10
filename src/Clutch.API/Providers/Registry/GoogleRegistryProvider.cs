using RestSharp;
using System.Buffers;

namespace Clutch.API.Providers.Registry
{
    public class GoogleRegistryProvider(IRestClientFactory restClientFactory, ArrayPool<byte> arrayPool, ILogger logger, IConfiguration configuration) : RegistryProviderBase(restClientFactory, logger, configuration)
    {
        public override async Task<RegistryManifestModel> GetManifestAsync(ContainerImageRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
