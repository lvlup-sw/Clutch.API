using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using RestSharp;
using Azure.Provisioning.ResourceManager;
using Azure.Core;

namespace Clutch.API.Providers.Registry
{
    // Process:
    // Obtain tenant id, client id, and client secret to generate tokens
    // We may use service principal to generate these secrets
    // Then we can use the ACR REST api to generate tokens and send a request

    public class AzureRegistryProvider(IRestClientFactory restClientFactory, ILogger logger, IConfiguration configuration) : RegistryProviderBase(restClientFactory, logger, configuration)
    {
        private readonly ILogger _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private IRestClientFactory _restClientFactory = restClientFactory;
        private readonly string azureToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(configuration.GetConnectionString("AzureToken") ?? string.Empty));

        public override async Task<RegistryManifestModel> GetManifestAsync(ContainerImageRequest request)
        {
            string[] parts = request.Repository.Split('/');

            // Construct the API request
            _restClientFactory.InstantiateClient("https://registry-1.docker.io");
            RestRequest restRequest = new($"/v2/{parts[0]}/{parts[1]}/manifests/{request.Tag}");
            restRequest.Method = Method.Get;
            restRequest.AddHeader("Accept", "application/vnd.docker.distribution.manifest.v2+json");
            restRequest.AddHeader("Authorization", $"Bearer {azureToken}");

            // Send the request
            RestResponse? response = await _restClientFactory.ExecuteAsync(restRequest);

            return ValidateAndDeserializeResponse(response);
        }
    }
}
