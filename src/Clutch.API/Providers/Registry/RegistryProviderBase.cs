using Clutch.API.Models.Image;
using Clutch.API.Models.Registry;
using Clutch.API.Properties;
using Clutch.API.Providers.Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System.Net;
using System.Text;

namespace Clutch.API.Providers.Registry
{
    // Implements GHCR
    public class RegistryProviderBase(IRestClientFactory restClientFactory, ILogger logger, IOptions<AppSettings> settings) : IRegistryProvider
    {
        private readonly ILogger _logger = logger;
        private readonly AppSettings _settings = settings.Value;
        private IRestClientFactory _restClientFactory = restClientFactory;
        private readonly string pat = Convert.ToBase64String(Encoding.UTF8.GetBytes(settings.Value.GithubPAT ?? string.Empty));

        public virtual async Task<RegistryManifestModel> GetManifestAsync(ContainerImageRequest request)
        {
            // Construct the request
            _restClientFactory.InstantiateClient("https://ghcr.io");
            string[] parts = request.Repository.Split('/');
            RestRequest restRequest = new($"/v2/{parts[0]}/{parts[1]}/manifests/{request.Tag}");
            restRequest.Method = Method.Get;
            restRequest.AddHeader("Accept", "application/vnd.github+json");
            restRequest.AddHeader("Authorization", $"Bearer {pat}");

            // Send the request
            RestResponse? response = await _restClientFactory.ExecuteAsync(restRequest);

            return ValidateAndDeserializeResponse(response);
        }

        // We can include protected helper methods here for common operations
        protected RegistryManifestModel ValidateAndDeserializeResponse(RestResponse? response)
        {
            if (response is null)
            {
                _logger.LogError("Failed to retrieve image manifest from registry due to an exception processing the request.");
                return RegistryManifestModel.Null;
            }
            else if (response.StatusCode != HttpStatusCode.OK)
            {
                _logger.LogError("Failed to retrieve image manifest from registry. StatusCode: {StatusCode}. ErrorMessage: {ErrorMessage}", response.StatusCode, response.ErrorMessage);
                return RegistryManifestModel.Null;
            }

            try
            {
                return JsonConvert.DeserializeObject<RegistryManifestModel>(response.Content!) ?? RegistryManifestModel.Null;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to deserialize image manifest. Exception: {ex}", ex.GetBaseException());
                return RegistryManifestModel.Null;
            }
        }
    }
}