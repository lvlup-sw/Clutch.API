// Service Layer:
// - "Business logic" for our API.
// - Interacts with all our Provider classes.

namespace Clutch.API.Services.Image
{
    public class ContainerImageService(ICacheProvider<ContainerImageModel> cacheProvider, IContainerImageProvider imageProvider,IRegistryProviderFactory registryProviderFactory, IEventPublisher eventPublisher, ILogger logger, IOptions<AppSettings> settings) : IContainerImageService
    {
        private readonly ICacheProvider<ContainerImageModel> _cacheProvider = cacheProvider;
        private readonly IContainerImageProvider _imageProvider = imageProvider;
        private readonly IRegistryProviderFactory _registryProviderFactory = registryProviderFactory;
        private readonly IEventPublisher _eventPublisher = eventPublisher;
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

            // This is primarily handling the case where we have an image in the DB
            // But it is not available in the container registry; either because it
            // is still building, deprecated, etc
            // We send a 202 with a message describing this situation
            if (image.Status is not StatusEnum.Available)
            {
                _logger.LogWarning("Image found but is not available.");
                return new ContainerImageResponseData(false, image, RegistryManifestModel.Null);
            }

            // Check the registry and construct the RegistryManifest
            // We utilize a Using block since the registry factory
            // creates an instance of the class we require, which we
            // need to dispose of afterwards.
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
            bool success = await _cacheProvider.SetInCacheAsync(cacheKey, image);

            if (!success)
            {
                _logger.LogError("Failed to set image in DB and/or Cache.");
                return false;
            }

            // Now that we have the image set in the DB & Cache, we publish an event
            // requesting a build to be generated. This will be sent to AZB and
            // consumed by our CI/CD pipeline.
            bool eventPublished = await _eventPublisher.PublishEventAsync("BuildRequested", image);

            if (!eventPublished)
            {
                _logger.LogError("Failed to publish SET event for image: {image.RepositoryId}.", image.RepositoryId);
            }

            return eventPublished;
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

            // If we have a valid model, try to delete from the database
            bool success = await _cacheProvider.RemoveFromCacheAsync(cacheKey);

            if (!success)
            {
                _logger.LogError("Failed to delete image from DB and/or Cache.");
                return false;
            }

            // Now that we have the image deleted from the DB & Cache, we publish an event
            // requesting the image to be deleted from the registry. This will be sent to
            // AZB and consumed by our CI/CD pipeline.
            ContainerImageModel image = ConstructImageModel(request, version);
            bool eventPublished = await _eventPublisher.PublishEventAsync("DeleteRequested", image);

            if (!eventPublished)
            {
                _logger.LogError("Failed to publish DEL event for image: {image.RepositoryId}.", image.RepositoryId);
            }

            return eventPublished;
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
                    BuildDate = DateTime.UtcNow,
                    RegistryType = request.RegistryType,
                    Status = StatusEnum.Unavailable,
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