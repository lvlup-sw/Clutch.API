using Clutch.API.Models.Image;
using Clutch.API.Models.Registry;
using Clutch.API.Properties;
using Clutch.API.Providers.Interfaces;
using Microsoft.Extensions.Options;
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
