using CacheProvider.Providers.Interfaces;
using Microsoft.Extensions.Options;
using Clutch.API.Models.Image;
using Clutch.API.Properties;
using Clutch.API.Providers.Interfaces;
using Clutch.API.Services.Interfaces;
using Newtonsoft.Json;
using RestSharp;
using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using System.Text;

// Service Responsibilities:
// - "Business logic" for image management.
// - CacheProvider interaction for Redis caching.
// - Interacting with the Github API to trigger builds.
// - Interacting with the Container Registry.
namespace Clutch.API.Services.Image
{
    public class ContainerImageService(ICacheProvider<ContainerImageModel> cacheProvider, IContainerImageProvider imageProvider, ILogger logger, IOptions<AppSettings> settings) : IContainerImageService
    {
        private readonly ICacheProvider<ContainerImageModel> _cacheProvider = cacheProvider;
        private readonly IContainerImageProvider _imageProvider = imageProvider;
        private readonly ILogger _logger = logger;
        private readonly AppSettings _settings = settings.Value;
        private readonly RestClient _restClient = new("https://ghcr.io");
        private readonly string pat = Convert.ToBase64String(Encoding.UTF8.GetBytes(settings.Value.GithubPAT!));
        private const string org = "lvlup-sw";
        private const string repository = "clutchapi";

        public async Task<ContainerImageResponseData> GetImageAsync(string imageReference)
        {
            // Retrieve from cacheProvider (which will call the imageProvider if not found)
            var image = await _cacheProvider.GetFromCacheAsync(imageReference);
            if (image is null || !image.HasValue)
            {
                _logger.LogError("Image not found in database.");
                return new ContainerImageResponseData(false, ContainerImageModel.Null, RegistryManifest.Null);
            }

            // Check the registry and construct the RegistryManifest
            // lvlup-sw/clutchapi:dev
            RegistryManifest manifest = await GetImagePropertiesFromRegistry(image.ImageReference);
            if (manifest is null || !manifest.HasValue)
            {
                _logger.LogError("Image not found in registry.");
                return new ContainerImageResponseData(false, ContainerImageModel.Null, RegistryManifest.Null);
            }
            
            return new ContainerImageResponseData(true, image, manifest);
        }

        public async Task<bool> SetImageAsync(ContainerImageModel containerImageModel)
        {
            // Before continuing we validate the image model
            if (!ValidateImageModel(containerImageModel))
            {
                _logger.LogError("Failed to construct the image model.");
                return false;
            }

            return await SetImageInRegistry(containerImageModel)
                && await _imageProvider.SetImageAsync(containerImageModel);
        }

        public async Task<bool> DeleteImageAsync(string imageReference)
        {
            return await DeleteFromRegistryAsync(imageReference) 
                && await _imageProvider.DeleteFromDatabaseAsync(imageReference);
        }

        public async Task<IEnumerable<ContainerImageModel>?> GetLatestImagesAsync()
        {
            // TODO: Figure out a caching implementation for this
            return await _imageProvider.GetLatestImagesAsync();
        }

        private bool ValidateImageModel(ContainerImageModel image)
        {
            // We need to validate the image model before proceeding
            // This includes checking the image reference, game version, and other properties
            // We need to introduce logic to construct the following properties:
            // - BuildDate
            // - Status
            // - ServerConfig

            return true;
        }

        // Registry interactions
        private async Task<RegistryManifest> GetImagePropertiesFromRegistry(string imageReference)
        {
            // Construct the request
            string tag = imageReference.Split(":")[1];
            RestRequest request = new($"/v2/{org}/{repository}/manifests/{tag}");
            request.AddHeader("Accept", "application/vnd.github+json");
            request.AddHeader("Authorization", $"Bearer {pat}");

            // Send the request
            RestResponse response = await _restClient.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                _logger.LogError("Failed to retrieve image manifest from registry. StatusCode: {StatusCode}. ErrorMessage: {ErrorMessage}", response.StatusCode, response.ErrorMessage);
                return RegistryManifest.Null;
            }

            try
            {
                return JsonConvert.DeserializeObject<RegistryManifest>(response.Content!) ?? RegistryManifest.Null;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to deserialize image manifest. Exception: {ex}", ex.GetBaseException());
                return RegistryManifest.Null;
            }
        }

        private async Task<bool> SetImageInRegistry(ContainerImageModel image)
        {
            // We need to handle the following scenarios:
            // - Image already exists in the registry
            // - Image does not exist in the registry
            //   + If image does not exist, we need to trigger the build pipeline
            throw new NotImplementedException();
        }

        private async Task<bool> DeleteFromRegistryAsync(string imageReference)
        {
            // We need to call the GitHub API to delete the image from the registry.
            // We will need to authenticate with a token.

            return true;
        }

        public async Task<ContainerImageBuildResult> TriggerImageBuildAsync(BuildParameters parameters)
        {
            // Construct the image model from the build parameters
            ContainerImageModel imageModel = new()
            {
                ImageID = 0,
                ImageReference = parameters.ImageReference,
                GameVersion = parameters.GameVersion,
                BuildDate = parameters.BuildDate,
                RegistryURL = parameters.RegistryURL,
                Status = StatusEnum.Building,
            };

            // Trigger the build pipeline
            // We do this by sending a POST request to the GitHub API
            return await SendPostRequest(imageModel, parameters);
        }

        private async Task<ContainerImageBuildResult> SendPostRequest(ContainerImageModel imageModel, BuildParameters parameters)
        {
            return new ContainerImageBuildResult
            {
                Success = true,
                ContainerImageModel = imageModel
            };
        }
    }
}
