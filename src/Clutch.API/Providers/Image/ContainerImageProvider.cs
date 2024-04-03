using Polly;
using Polly.Wrap;
using Microsoft.Extensions.Options;
using Clutch.API.Models.Image;
using Clutch.API.Properties;
using Clutch.API.Providers.Interfaces;
using Clutch.API.Repositories.Interfaces;

// Provider Responsibilities:
// - Interacting with the Repositories to perform operations against the database.
// - Triggering our CI/CD pipeline to build a new image.
// - Interacting with any other external services for the purpose of getting data.
// - DOES NOT directly manipulate the database; that is the repository's job.
namespace Clutch.API.Providers.Image
{
    // In the future, we will add other Repository interfaces to this class.
    // For now, we only have the ContainerImageRepository class so this acts
    // as a pass-through with polly decorations.
    public class ContainerImageProvider : IContainerImageProvider
    {
        private readonly IContainerImageRepository _repository;
        private readonly ILogger _logger;
        private readonly AppSettings _settings;
        private readonly AsyncPolicyWrap<object> _policy;

        public ContainerImageProvider(IContainerImageRepository repository, ILogger logger, IOptions<AppSettings> settings)
        {
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(settings);

            _repository = repository;
            _logger = logger;
            _settings = settings.Value;
            _policy = CreatePolicy();
        }

        public async Task<ContainerImageModel?> GetImageByIdAsync(int imageId)
        {
            object result = await _policy.ExecuteAsync(async (context) =>
            {
                _logger.LogDebug("Attempting to retrieve entry with ID {imageId} from database.", imageId);
                ContainerImageModel data = await _repository.GetImageByIdAsync(imageId);
                return data.HasValue ? data : ContainerImageModel.Null;
            }, new Context($"ContainerImageProvider.GetImageByIdAsync for Image ID: {imageId}"));

            return result is ContainerImageModel image && image.HasValue 
                ? image
                : null;
        }

        public async Task<ContainerImageModel?> GetImageByReferenceAsync(string imageReference)
        {
            object result = await _policy.ExecuteAsync(async (context) =>
            {
                _logger.LogDebug("Attempting to retrieve entry with ref {imageReference} from database.", imageReference);
                ContainerImageModel data = await _repository.GetImageByReferenceAsync(imageReference);
                return data.HasValue ? data : ContainerImageModel.Null;
            }, new Context($"ContainerImageProvider.GetImageByReferenceAsync for Image Ref: {imageReference}"));

            return result is ContainerImageModel image && image.HasValue
                ? image
                : null;
        }

        public async Task<IEnumerable<ContainerImageModel>?> GetLatestImagesAsync()
        {
            object result = await _policy.ExecuteAsync(async (context) =>
            {
                _logger.LogDebug("Attempting to retrieve all entries from the database.");
                var data = await _repository.GetLatestImagesAsync();
                return data.Any() ? data : [];
            }, new Context($"ContainerImageProvider.GetLatestImagesAsync"));

            return result is IEnumerable<ContainerImageModel> images && images.Any()
                ? images
                : null;
        }

        public async Task<bool> SetImageAsync(ContainerImageModel image)
        {
            object result = await _policy.ExecuteAsync(async (context) =>
            {
                _logger.LogDebug("Attempting to set image in the database.");
                return await _repository.SetImageAsync(image);
            }, new Context($"ContainerImageProvider.SetImageAsync for Image Ref: {image.ImageReference}"));

            // TODO: We need to trigger the build pipeline before returning.
            return result is bool success
                ? success
                : default;
        }

        public async Task<bool> DeleteFromDatabaseAsync(string imageReference)
        {
            object result = await _policy.ExecuteAsync(async (context) =>
            {
                _logger.LogDebug("Attempting to delete image from the database.");
                return await _repository.DeleteImageAsync(imageReference);
            }, new Context($"ContainerImageProvider.DeleteImageAsync for Image Ref: {imageReference}"));

            return result is bool success
                ? success
                : default;
        }

        public async Task<bool> DeleteFromDatabaseAsync(int imageId)
        {
            object result = await _policy.ExecuteAsync(async (context) =>
            {
                _logger.LogDebug("Attempting to delete image from the database.");
                return await _repository.DeleteImageAsync(imageId);
            }, new Context($"ContainerImageProvider.DeleteImageAsync for Image ID: {imageId}"));

            return result is bool success
                ? success
                : default;
        }

        public async Task<ContainerImageBuildResult> TriggerBuildAsync(BuildParameters parameters)
        {
            // We need to call our GHA workflow to trigger a new build.
            // This should output to the GH container registry.

            // First, we need to construct the ContainerImageModel from the BuildParameters.
            // Then, we need to call the SetImageAsync method to store the new image in the database.
            // Finally, we need to trigger the build pipeline.

            // Construct the image model
            ContainerImageModel imageModel = new()
            {
                ImageID = 0,
                ImageReference = parameters.ImageReference,
                GameVersion = parameters.GameVersion,
                BuildDate = parameters.BuildDate,
                RegistryURL = parameters.RegistryURL,
                Status = StatusEnum.Building,
            };

            // Set the image in the database
            bool imageWasSet = await SetImageAsync(imageModel);
            if (!imageWasSet)
            {
                _logger.LogError("Failed to set image in the database.");
                return new ContainerImageBuildResult
                {
                    Success = false,
                    ContainerImageModel = ContainerImageModel.Null,
                };
            }

            // Trigger the build pipeline
            // We do this by sending a POST request to the GitHub API
            return null;
        }

        public async Task<bool> DeleteFromRegistryAsync(string imageReference)
        {
            // We need to call the GitHub API to delete the image from the registry.
            // We will need to authenticate with a token.

            return true;
        }

        // CacheProvider method
        public async Task<ContainerImageModel?> GetAsync(string key) => await GetImageByReferenceAsync(key);

        // CacheProvider method
        public async Task<Dictionary<string, ContainerImageModel?>> GetBatchAsync(IEnumerable<string> keys, CancellationToken? cancellationToken = null)
        {
            throw new NotImplementedException();
        }

        private AsyncPolicyWrap<object> CreatePolicy()
        {
            // Retry Policy Settings:
            // + RetryCount: The number of times to retry a cache operation.
            // + RetryInterval: The interval between cache operation retries.
            // + UseExponentialBackoff: Set to true to use exponential backoff for cache operation retries.
            var retryPolicy = Policy<object>
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: _settings.ProviderRetryCount,
                    // Jitter and either exponential backoff or fixed interval
                    sleepDurationProvider: retryAttempt => _settings.ProviderUseExponentialBackoff
                        ? TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                        : TimeSpan.FromSeconds(_settings.ProviderRetryInterval)
                            + TimeSpan.FromMilliseconds(new Random().Next(0, 100)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        if (retryCount == _settings.ProviderRetryCount)
                        {
                            _logger.LogError($"Retry limit of {_settings.ProviderRetryCount} reached. Exception: {exception}");
                        }
                        else
                        {
                            _logger.LogInformation($"Retry {retryCount} of {_settings.ProviderRetryCount} after {timeSpan.TotalSeconds} seconds delay due to: {exception}");
                        }
                    });

            // Fallback Policy Settings:
            // + FallbackValue: The value to return if the fallback action is executed.
            var fallbackPolicy = Policy<object>
                .Handle<Exception>()
                .FallbackAsync(
                    fallbackValue: ContainerImageModel.Null,
                    onFallbackAsync: (exception, context) =>
                    {
                        _logger.LogError("Fallback executed due to: {exception}", exception);
                        return Task.CompletedTask;
                    });

            return Policy.WrapAsync(retryPolicy, fallbackPolicy);
        }
    }
}
