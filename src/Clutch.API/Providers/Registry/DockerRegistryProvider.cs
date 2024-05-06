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
    public class DockerRegistryProvider(ILogger logger, IOptions<AppSettings> settings) : RegistryProviderBase(logger, settings)
    {
        private readonly ILogger _logger = logger;
        private readonly AppSettings _settings = settings.Value;
        private readonly RestClient _restClient = new("https://registry-1.docker.io");
        private readonly RestClient _authClient = new("https://auth.docker.io");
        // Unused; only for private repos
        private readonly string pat = Convert.ToBase64String(Encoding.UTF8.GetBytes(settings.Value.DockerPAT!));

        public override async Task<RegistryManifestModel> GetManifestAsync(ContainerImageRequest request)
        {
            string[] parts = request.Repository.Split('/');
            dynamic rawToken;

            // Get the Bearer Token
            RestRequest authRequest = new($"/token?service=registry.docker.io&scope=repository:{parts[0]}/{parts[1]}:pull");
            authRequest.Method = Method.Get;
            RestResponse authResponse = await _authClient.ExecuteAsync(authRequest);

            if (!authResponse.IsSuccessful)
            {
                _logger.LogError("Failed to retrieve bearer token for registry. StatusCode: {StatusCode}. ErrorMessage: {ErrorMessage}", authResponse.StatusCode, authResponse.ErrorMessage);
                return RegistryManifestModel.Null;
            }

            try
            {
                rawToken = JsonConvert.DeserializeObject(authResponse.Content!) ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to deserialize bearer token. Exception: {ex}", ex.GetBaseException());
                return RegistryManifestModel.Null;
            }

            // Construct the API request
            RestRequest restRequest = new($"/v2/{parts[0]}/{parts[1]}/manifests/{request.Tag}");
            restRequest.Method = Method.Get;
            restRequest.AddHeader("Accept", "application/vnd.docker.distribution.manifest.v2+json");
            restRequest.AddHeader("Authorization", $"Bearer {rawToken.token}");

            // Send the request
            RestResponse response = await _restClient.ExecuteAsync(restRequest);

            return ValidateAndDeserializeResponse(response);
        }
    }
}
