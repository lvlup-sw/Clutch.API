using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using RestSharp;
using Azure.Provisioning.ResourceManager;
using Azure.Core;
using Amazon.Runtime.Internal;
using Google.Apis.Auth.OAuth2;

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
        // Azure auth
        private readonly string tenantId = configuration["Azure:AzureTenantId"] ?? string.Empty;
        private readonly string azureClientId = configuration["Azure:AzureClientId"] ?? string.Empty;
        private readonly string azureClientSecret = configuration["Secrets:AzureClientSecret"] ?? string.Empty;
        private readonly string azureScope = configuration["Azure:AzureScope"] ?? string.Empty;

        public override async Task<RegistryManifestModel> GetManifestAsync(ContainerImageRequest request)
        {
            string[] parts = request.Repository.Split('/');
            dynamic azureToken = await GetToken(parts);

            // Construct the API request
            _restClientFactory.InstantiateClient("https://containerimagesregistry.azurecr.io");
            // This should be correct, but it's hard to say without pushing an image first
            RestRequest restRequest = new($"/v2/{parts[0]}/{parts[1]}/manifests/{request.Tag}");
            restRequest.Method = Method.Get;
            restRequest.AddHeader("Accept", "application/vnd.docker.distribution.manifest.v2+json");
            restRequest.AddHeader("Authorization", $"Bearer {azureToken.access_token}");

            // Send the request
            RestResponse? response = await _restClientFactory.ExecuteAsync(restRequest);

            return ValidateAndDeserializeResponse(response);
        }

        private async Task<dynamic> GetToken(string[] parts)
        {
            dynamic rawToken;

            // Get the Access Token
            _restClientFactory.InstantiateClient("https://login.microsoftonline.com");
            RestRequest authRequest = new($"/{tenantId}/oauth2/v2.0/token");
            authRequest.Method = Method.Post;
            authRequest.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            authRequest.AddParameter("grant_type", "client_credentials");
            authRequest.AddParameter("client_id", azureClientId);
            authRequest.AddParameter("client_secret", azureClientSecret);
            authRequest.AddParameter("scope", azureScope);

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
