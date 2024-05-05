using Clutch.API.Models.Image;
using Clutch.API.Properties;
using Clutch.API.Providers.Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System.Text;

namespace Clutch.API.Providers.Image
{
    // Implements GHCR
    public class RegistryProviderBase(ILogger logger, IOptions<AppSettings> settings) : IRegistryProvider
    {
        private readonly ILogger _logger = logger;
        private readonly AppSettings _settings = settings.Value;
        private readonly RestClient _restClient = new("https://ghcr.io");
        private readonly string pat = Convert.ToBase64String(Encoding.UTF8.GetBytes(settings.Value.GithubPAT!));

        public async Task<RegistryManifestModel> GetManifestAsync(ContainerImageRequest request)
        {
            // Construct the request
            string[] parts = request.Repository.Split('/');
            RestRequest restRequest = new($"/v2/{parts[0]}/{parts[1]}/manifests/{request.Tag}");
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

        public async Task<bool> SetManifestAsync(ContainerImageRequest request)
        {
            // We will trigger the build pipeline here

            return true;
        }

        public async Task<bool> DeleteManifestAsync(ContainerImageRequest request)
        {
            // We will call the api here

            return true;
        }

        public async Task<IEnumerable<ContainerImageModel>?> GetLatestManifestsAsync()
        {
            throw new NotImplementedException();
        }

        // We can include protected helper methods here for common operations
    }
}