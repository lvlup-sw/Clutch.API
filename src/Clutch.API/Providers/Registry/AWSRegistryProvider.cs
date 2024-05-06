using Clutch.API.Models.Image;
using Clutch.API.Models.Registry;
using Clutch.API.Properties;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System.IO;
using System.Text;

namespace Clutch.API.Providers.Registry
{
    public class AWSRegistryProvider(ILogger logger, IOptions<AppSettings> settings) : RegistryProviderBase(logger, settings)
    {
        public override async Task<RegistryManifestModel> GetManifestAsync(ContainerImageRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
