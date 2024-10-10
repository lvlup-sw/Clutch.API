using Polly;
using Polly.Wrap;

// Provider Responsibilities:
// - Interacting with the Repositories to perform operations against the database.
// - DOES NOT directly manipulate the database; that is the repository's job.
namespace Clutch.API.Providers.Image
{
    // In the future, we may add other repository interfaces to this class.
    // For now, we only have the ContainerImageRepository class so this acts
    // as a pass-through with polly decorations.
    public class ContainerImageProvider : IContainerImageProvider
    {
        private readonly IContainerImageRepository _repository;
        private readonly ILogger _logger;
        private readonly AppSettings _settings;
        public AsyncPolicyWrap<object> Policy { get; set; }

        public ContainerImageProvider(IContainerImageRepository repository, ILogger logger, IOptions<AppSettings> settings)
        {
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(settings);

            _repository = repository;
            _logger = logger;
            _settings = settings.Value;
            Policy = GetDefaultPattern();
        }

        public async Task<ContainerImageModel?> GetImageAsync(int imageId)
        {
            if (imageId < 1) return null;

            object result = await Policy.ExecuteAsync(async (context) =>
            {
                _logger.LogDebug("Attempting to retrieve entry with ID {imageId} from database.", imageId);
                ContainerImageModel data = await _repository.GetImageAsync(imageId);
                return data.HasValue ? data : ContainerImageModel.Null;
            }, new Context($"ContainerImageProvider.GetImageAsync for Image ID: {imageId}"));

            return result is ContainerImageModel image && image.HasValue
                ? image
                : null;
        }

        public async Task<ContainerImageModel?> GetImageAsync(string key)
        {
            // Extract repository and tag from key
            string repositoryId = ExtractIdFromKey(key);
            if (string.IsNullOrEmpty(repositoryId)) return null;

            object result = await Policy.ExecuteAsync(async (context) =>
            {
                _logger.LogDebug("Attempting to retrieve entry with ref {repositoryId} from database.", repositoryId);
                ContainerImageModel data = await _repository.GetImageAsync(repositoryId);
                return data.HasValue ? data : ContainerImageModel.Null;
            }, new Context($"ContainerImageProvider.GetImageAsync for Image Ref: {repositoryId}"));

            return result is ContainerImageModel image && image.HasValue
                ? image
                : null;
        }

        public async Task<IEnumerable<ContainerImageModel>?> GetLatestImagesAsync()
        {
            object result = await Policy.ExecuteAsync(async (context) =>
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
            if (!image.HasValue) return false;

            object result = await Policy.ExecuteAsync(async (context) =>
            {
                _logger.LogDebug("Attempting to set image in the database.");
                return await _repository.SetImageAsync(image);
            }, new Context($"ContainerImageProvider.SetImageAsync for Image Ref: {image.Repository}"));

            return result is bool success
                ? success
                : default;
        }

        public async Task<bool> DeleteImageAsync(string key)
        {
            // Extract repository and tag from key
            string repositoryId = ExtractIdFromKey(key);
            if (string.IsNullOrEmpty(repositoryId)) return false;

            object result = await Policy.ExecuteAsync(async (context) =>
            {
                _logger.LogDebug("Attempting to delete image from the database.");
                return await _repository.DeleteImageAsync(repositoryId);
            }, new Context($"ContainerImageProvider.DeleteImageAsync for Image Ref: {repositoryId}"));

            return result is bool success
                ? success
                : default;
        }

        public async Task<bool> DeleteImageAsync(int imageId)
        {
            if (imageId < 1) return false;

            object result = await Policy.ExecuteAsync(async (context) =>
            {
                _logger.LogDebug("Attempting to delete image from the database.");
                return await _repository.DeleteImageAsync(imageId);
            }, new Context($"ContainerImageProvider.DeleteImageAsync for Image ID: {imageId}"));

            return result is bool success
                ? success
                : default;
        }

        // CacheProvider method
        public async Task<ContainerImageModel?> GetFromSourceAsync(string key) => await GetImageAsync(key);

        // CacheProvider method
        public async Task<bool> SetInSourceAsync(ContainerImageModel data) => await SetImageAsync(data);

        // CacheProvider method
        public async Task<bool> DeleteFromSourceAsync(string key) => await DeleteImageAsync(key);

        // CacheProvider method
        public Task<IDictionary<string, ContainerImageModel>> GetBatchFromSourceAsync(IEnumerable<string> keys, CancellationToken? cancellationToken = null)
            => throw new NotImplementedException();

        public Task<IDictionary<string, bool>> SetBatchInSourceAsync(IDictionary<string, ContainerImageModel> data, CancellationToken? cancellationToken = null)
            => throw new NotImplementedException();

        public Task<IDictionary<string, bool>> RemoveBatchFromSourceAsync(IEnumerable<string> keys, CancellationToken? cancellationToken = null)
            => throw new NotImplementedException();

        private string ExtractIdFromKey(string key)
        {
            try
            {
                // Expected key format: {version}:{repository}:{tag}:{hash}
                string[] keyItems = key.Split(':');
                if (keyItems.Length != 4) throw new Exception();
                return $"{keyItems[1]}:{keyItems[2]}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to extract valid Id from cache key: {key}", key);
                return string.Empty;
            }
        }

        private static AsyncPolicyWrap<object> GetDefaultPattern()
        {
            var timeoutPolicy = Polly.Policy.TimeoutAsync(
                TimeSpan.FromSeconds(30));

            var fallbackPolicy = Policy<object>
                .Handle<Exception>()
                .FallbackAsync(
                    fallbackValue: string.Empty,
                    onFallbackAsync: (exception, context) =>
                    {
                        return Task.CompletedTask;
                    });

            return fallbackPolicy.WrapAsync(timeoutPolicy);
        }
    }
}
