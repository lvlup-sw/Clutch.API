using CacheProvider.Providers.Interfaces;
using Microsoft.Extensions.Options;
using StepNet.API.Models.Image;
using StepNet.API.Properties;
using StepNet.API.Providers.Interfaces;
using StepNet.API.Services.Interfaces;

// Service Responsibilities:
// - "Business logic" for image management.
// - CacheProvider interaction for Redis caching.
namespace StepNet.API.Services.Image
{
    public class ContainerImageService(ICacheProvider<ContainerImageModel> cacheProvider, IContainerImageProvider imageProvider, ILogger logger, IOptions<AppSettings> settings) : IContainerImageService
    {
        private readonly ICacheProvider<ContainerImageModel> _cacheProvider = cacheProvider;
        private readonly IContainerImageProvider _imageProvider = imageProvider;
        private readonly ILogger _logger = logger;
        private readonly AppSettings _settings = settings.Value;

        public async Task<ContainerImageResponseData> GetImageAsync(string imageReference)
        {
            // Retrieve from cacheProvider (which will call the imageProvider if not found)
            var image = await _cacheProvider.GetFromCacheAsync(imageReference);
            if (image is null)
            {
                _logger.LogError("Image not found.");
                return new ContainerImageResponseData(false, new RegistryProperties(), ContainerImageModel.Null);
            }

            // Check the registry and construct the RegistryProperties
            RegistryProperties registryProperties = await GetPropertiesFromRegistry(image.ImageReference);
            if (registryProperties is null)
            {
                _logger.LogError("Registry properties not found.");
                return new ContainerImageResponseData(false, new RegistryProperties(), ContainerImageModel.Null);
            }
            
            return new ContainerImageResponseData(true, registryProperties, image);
        }

        public async Task<bool> SetImageAsync(ContainerImageModel containerImageModel)
        {
            // We need to introduce logic to construct the following properties:
            // - BuildDate
            // - Status
            // - Layers
            // - Size
            // - CommitHash
            // - ServerConfig

            // First, we validate and construct the image model
            // Call an image constructor method here

            // Before continuing we validate the image model
            if (containerImageModel.ImageID != 0)
            {
                _logger.LogInformation("Image ID must be 0 for new images.");
                return false;
            }

            // Next, we need to check the registry to see if the image already exists
            // If not and the image is valid, we trigger the build pipeline

            // This is the last step in the process
            // We add the image to the database
            return await _imageProvider.SetImageAsync(containerImageModel);
        }

        public async Task<bool> DeleteImageAsync(string imageReference)
        {
            // We need to introduce logic to delete the image from the registry
            // Therefore we need to call a method to delete the image from the registry
            // This will also be at the provider level

            return await _imageProvider.DeleteFromRegistryAsync(imageReference) 
                && await _imageProvider.DeleteFromDatabaseAsync(imageReference);
        }

        public async Task<IEnumerable<ContainerImageModel>?> GetLatestImagesAsync()
        {
            // TODO: Figure out a caching implementation for this
            return await _imageProvider.GetLatestImagesAsync();
        }

        private async Task<RegistryProperties> GetPropertiesFromRegistry(string imageReference)
        {
            throw new NotImplementedException();
        }
    }
}
