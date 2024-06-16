using Clutch.API.Models.Enums;
using Clutch.API.Models.Image;
using Clutch.API.Models.Registry;
using Clutch.API.Properties;
using Clutch.API.Providers.Interfaces;
using Clutch.API.Services.Interfaces;
using Clutch.API.Utilities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

// Service Responsibilities:
// - "Business logic" for image management.
// - CacheProvider interaction for Redis caching.
// - Interacting with the Github API to trigger builds.
// - Interacting with the Container Registry.
namespace Clutch.API.Services.Image
{
    public class ContainerImageService(ICacheProvider<ContainerImageModel> cacheProvider, IContainerImageProvider imageProvider, IRegistryProviderFactory registryProviderFactory, ILogger logger, IOptions<AppSettings> settings) : IContainerImageService
    {
        private readonly ICacheProvider<ContainerImageModel> _cacheProvider = cacheProvider;
        private readonly IContainerImageProvider _imageProvider = imageProvider;
        private readonly IRegistryProviderFactory _registryProviderFactory = registryProviderFactory;
        private readonly ILogger _logger = logger;
        private readonly AppSettings _settings = settings.Value;

        public async Task<ContainerImageResponseData> GetImageAsync(ContainerImageRequest request, string version)
        {
            // Construct the cache key from the parameters
            string cacheKey = ConstructCacheKey(request, version);
            if (string.IsNullOrEmpty(cacheKey)) 
            {
                _logger.LogError("Constructed cache key was empty or null.");
                return new ContainerImageResponseData(false, ContainerImageModel.Null, RegistryManifestModel.Null);
            }

            // Retrieve from cacheProvider (which will call the imageProvider if not found)
            ContainerImageModel? image = await _cacheProvider.GetFromCacheAsync(cacheKey);
            if (image is null || !image.HasValue)
            {
                _logger.LogError("Image not found in database.");
                return new ContainerImageResponseData(false, ContainerImageModel.Null, RegistryManifestModel.Null);
            }

            // Check the registry and construct the RegistryManifest
            // We utilize a Using block since the registry factory
            // creates an instance of the class we require, which we
            // need to dispose afterwards.
            using (IRegistryProvider? registryProvider = _registryProviderFactory.CreateRegistryProvider(request.RegistryType))
            {
                if (registryProvider is null)
                {
                    _logger.LogError("RegistryProviderFactory returned a null instance.");
                    return new ContainerImageResponseData(false, ContainerImageModel.Null, RegistryManifestModel.Null);
                }

                RegistryManifestModel manifest = await registryProvider.GetManifestAsync(request);
                if (manifest is null || !manifest.HasValue)
                {
                    _logger.LogError("Image not found in registry.");
                    return new ContainerImageResponseData(false, ContainerImageModel.Null, RegistryManifestModel.Null);
                }

                return new ContainerImageResponseData(true, image, manifest);
            };
        }

        public async Task<bool> SetImageAsync(ContainerImageRequest request, string version)
        {
            // Construct the cache key from the parameters
            string cacheKey = ConstructCacheKey(request, version);
            if (string.IsNullOrEmpty(cacheKey))
            {
                _logger.LogError("Constructed cache key was empty or null.");
                return false;
            }

            // Construct the image model from the request
            ContainerImageModel image = ConstructImageModel(request, version);
            if (image is null || !image.HasValue)
            {
                _logger.LogError("Failed to construct the image model.");
                return false;
            }

            // If we have a valid model, try to set in the database
            return await _cacheProvider.SetInCacheAsync(cacheKey, image);
        }

        public async Task<bool> DeleteImageAsync(ContainerImageRequest request, string version)
        {
            // Construct the cache key from the parameters
            string cacheKey = ConstructCacheKey(request, version);
            if (string.IsNullOrEmpty(cacheKey))
            {
                _logger.LogError("Constructed cache key was empty or null.");
                return false;
            }

            return await _cacheProvider.RemoveFromCacheAsync(cacheKey);
        }

        public async Task<IEnumerable<ContainerImageModel>?> GetLatestImagesAsync()
        {
            // TODO: Figure out a caching implementation for this
            // This is essentially going to be used by the UI to get a list of Images to select from
            return await _imageProvider.GetLatestImagesAsync();
        }

        private ContainerImageModel ConstructImageModel(ContainerImageRequest request, string version)
        {
            // Our pipeline will modify two values:
            // BuildDate and Status
            try
            {
                return new()
                {
                    ImageID = 0,
                    RepositoryId = $"{request.Repository}:{request.Tag}",
                    Repository = request.Repository,
                    Tag = request.Tag,
                    BuildDate = DateTime.Now,
                    RegistryType = request.RegistryType,
                    Status = StatusEnum.Available,
                    Version = version
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetBaseException(), "Error creating the image model to set in DB.");
                return ContainerImageModel.Null;
            }
        }

        private string ConstructCacheKey(ContainerImageRequest request, string version)
        {
            string prefix = $"{version}:{request.Repository}:{request.Tag}";

            try
            {
                return CacheKeyGenerator.GenerateCacheKey(request, prefix);
            } 
            catch (Exception ex)
            {
                _logger.LogError(ex.GetBaseException(), "Exception occurred while generating cache key.");
                return string.Empty;
            }
        }
    }
}