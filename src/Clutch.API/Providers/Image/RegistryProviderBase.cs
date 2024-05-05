using Clutch.API.Models.Image;
using Clutch.API.Properties;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System.Text;

namespace Clutch.API.Providers.Interfaces
{
    // Implements GHCR
    public class RegistryProviderBase(ILogger logger, IOptions<AppSettings> settings) : IRegistryProvider
    {
        private readonly ILogger _logger = logger;
        private readonly AppSettings _settings = settings.Value;
        private readonly RestClient _restClient = new("https://ghcr.io");
        private readonly string pat = Convert.ToBase64String(Encoding.UTF8.GetBytes(settings.Value.GithubPAT!));
        private const string org = "lvlup-sw";
        private const string repository = "clutchapi";

        public async Task<RegistryManifestModel> GetManifestAsync(ContainerImageRequest request)
        {
            // Construct the request
            RestRequest restRequest = new($"/v2/{org}/{request.Repository}/manifests/{request.Tag}");
            restRequest.AddHeader("Accept", "application/vnd.github+json");
            restRequest.AddHeader("Authorization", $"Bearer {pat}");

            // Send the request
            RestResponse response = await _restClient.ExecuteAsync(restRequest);

            if (!response.IsSuccessful)
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

        public Task<bool> SetManifestAsync(RegistryManifestModel manifest)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteManifestAsync(ContainerImageRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ContainerImageModel>?> GetLatestManifestsAsync()
        {
            throw new NotImplementedException();
        }

        // We can include protected helper methods here for common operations
    }
}