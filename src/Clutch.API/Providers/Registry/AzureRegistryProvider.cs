using Newtonsoft.Json;
using RestSharp;

namespace Clutch.API.Providers.Registry
{
    public class AzureRegistryProvider(IRestClientFactory restClientFactory, ILogger logger, IConfiguration configuration) : RegistryProviderBase(restClientFactory, logger, configuration)
    {
        private readonly ILogger _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private IRestClientFactory _restClientFactory = restClientFactory;
        // Azure auth
        private readonly string azureClientId = configuration["Azure:AzureClientId"] ?? string.Empty;
        private readonly string azureClientSecret = configuration["Secrets:AzureClientSecret"] ?? string.Empty;
        private readonly string azureService = configuration["Azure:AzureService"] ?? string.Empty;

        public override async Task<RegistryManifestModel> GetManifestAsync(ContainerImageRequest request)
        {
            string[] parts = request.Repository.Split('/');
            dynamic azureToken = await GetToken(parts);

            // Construct the API request
            // We should make this an appsetting
            _restClientFactory.InstantiateClient("https://containerimagesregistry.azurecr.io");
            RestRequest restRequest = new($"/v2/{parts[0]}/{parts[1]}/manifests/{request.Tag}");
            restRequest.Method = Method.Get;
            restRequest.AddHeader("Accept", "application/vnd.docker.distribution.manifest.v2+json");
            restRequest.AddHeader("Authorization", $"Bearer {azureToken.access_token}");

            // Send the request
            RestResponse? response = await _restClientFactory.ExecuteAsync(restRequest);

            return ValidateAndDeserializeResponse(response);
        }

        internal async Task<dynamic> GetToken(string[] parts)
        {
            dynamic rawToken;

            // Get the Access Token
            _restClientFactory.InstantiateClient($"https://{azureService}");
            RestRequest authRequest = new($"/oauth2/token");
            authRequest.Method = Method.Get;
            authRequest.AddParameter("service", azureService);
            authRequest.AddParameter("scope", $"repository:{parts[0]}/{parts[1]}:pull,push");
            authRequest.AddHeader("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{azureClientId}:{azureClientSecret}")));

            RestResponse? authResponse = await _restClientFactory.ExecuteAsync(authRequest);
            if (authResponse is null)
            {
                _logger.LogError("Failed to retrieve image manifest from registry due to an exception processing the request.");
                return RegistryManifestModel.Null;
            }
            else if (authResponse.StatusCode != HttpStatusCode.OK || authResponse.Content is null)
            {
                _logger.LogError("Failed to retrieve bearer token for registry. StatusCode: {StatusCode}. ErrorMessage: {ErrorMessage}", authResponse.StatusCode, authResponse.ErrorMessage);
                return RegistryManifestModel.Null;
            }

            try
            {
                rawToken = JsonConvert.DeserializeObject(authResponse.Content) ?? string.Empty;
                if (rawToken is string) throw new DeserializationException(authResponse, new("Deserialization returned null."));
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to deserialize bearer token. Exception: {ex}", ex.GetBaseException());
                return RegistryManifestModel.Null;
            }

            return rawToken;
        }
    }
}
