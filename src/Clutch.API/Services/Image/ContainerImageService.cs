using CacheProvider.Providers.Interfaces;
using Microsoft.Extensions.Options;
using Clutch.API.Models.Image;
using Clutch.API.Properties;
using Clutch.API.Providers.Interfaces;
using Clutch.API.Services.Interfaces;
using Newtonsoft.Json;
using System.Text;
using System.Security.Cryptography;
using Clutch.API.Models.Enums;
using Clutch.API.Models.Registry;

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

            // Retrieve from cacheProvider (which will call the imageProvider if not found)
            ContainerImageModel? image = await _cacheProvider.GetFromCacheAsync(cacheKey);
            if (image is null || !image.HasValue)
            {
                _logger.LogError("Image not found in database.");
                return new ContainerImageResponseData(false, ContainerImageModel.Null, RegistryManifestModel.Null);
            }

            // Check the registry and construct the RegistryManifest
            IRegistryProvider registryProvider = _registryProviderFactory.CreateRegistryProvider(request.RegistryType);
            RegistryManifestModel manifest = await registryProvider.GetManifestAsync(request);
            if (manifest is null || !manifest.HasValue)
            {
                _logger.LogError("Image not found in registry.");
                return new ContainerImageResponseData(false, ContainerImageModel.Null, RegistryManifestModel.Null);
            }
            
            return new ContainerImageResponseData(true, image, manifest);
        }

        public async Task<bool> SetImageAsync(ContainerImageRequest request, string version)
        {
            // First we need to construct the image model from the request
            ContainerImageModel image = ConstructImageModel(request, version);
            if (image is null || !image.HasValue)
            {
                _logger.LogError("Failed to construct the image model.");
                return false;
            }

            // If we have a valid model, try to set in the database
            return await _imageProvider.SetImageAsync(image);   // Change to cacheprovider
        }

        public async Task<bool> DeleteImageAsync(ContainerImageRequest request, string version)
        {
            // Construct the cache key from the parameters
            string cacheKey = ConstructCacheKey(request, version);

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
                return new ContainerImageModel()
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

        private static string ConstructCacheKey(ContainerImageRequest request, string version)
        {
            // Serialize and hash the request object
            byte[] hash = SHA256.HashData(
                Encoding.UTF8.GetBytes(
                    JsonConvert.SerializeObject(request)
                )
            );

            // Construct the cache key by converting each byte
            // in the hash into a two-character hexadecimal representation
            // We use the Version, Repository, and Tag as prefixes
            return $"{version}:{request.Repository}:{request.Tag}:{string.Join("", hash.Select(b => b.ToString("x2"))).ToLower()}";
        }
    }
}