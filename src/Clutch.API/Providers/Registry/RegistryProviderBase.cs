using RestSharp;
using System.Buffers;
using System.Text;

namespace Clutch.API.Providers.Registry
{
    // Implements GHCR
    public class RegistryProviderBase(IRestClientFactory restClientFactory, ArrayPool<byte> arrayPool, ILogger logger, IConfiguration configuration) : IRegistryProvider, IDisposable
    {
        private readonly ILogger _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private IRestClientFactory _restClientFactory = restClientFactory;
        private readonly ArrayPool<byte> _arrayPool = arrayPool;
        private readonly string pat = Convert.ToBase64String(Encoding.UTF8.GetBytes(configuration.GetConnectionString("GithubPAT") ?? string.Empty));

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

            return await ValidateAndDeserializeResponseAsync(response);
        }

        // We can include protected helper methods here for common operations
        protected async Task<RegistryManifestModel> ValidateAndDeserializeResponseAsync(RestResponse? response)
        {
            if (response is null)
            {
                _logger.LogError("Failed to retrieve image manifest from registry due to an exception processing the request.");
                return RegistryManifestModel.Null;
            }
            else if (response.StatusCode != HttpStatusCode.OK || response.Content is null)
            {
                _logger.LogError("Failed to retrieve image manifest from registry. StatusCode: {StatusCode}. ErrorMessage: {ErrorMessage}", response.StatusCode, response.ErrorMessage);
                return RegistryManifestModel.Null;
            }

            // Estimate the size of the byte array needed
            int estimatedSize = Encoding.UTF8.GetByteCount(response.Content);
            byte[] rentedArray = _arrayPool is not null
                ? _arrayPool.Rent(estimatedSize)
                : new byte[estimatedSize];

            try
            {
                // Setup the stream
                int bytesWritten = Encoding.UTF8.GetBytes(response.Content, 0, response.Content.Length, rentedArray, 0);
                using var memoryStream = new MemoryStream(rentedArray, 0, bytesWritten);

                // Deserialize
                var result = await JsonSerializer.DeserializeAsync<RegistryManifestModel>(memoryStream);

                if (result is not null) return result;

                return RegistryManifestModel.Null;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to deserialize image manifest. Exception: {ex}", ex.GetBaseException());
                return RegistryManifestModel.Null;
            }
        }

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // Currently we don't need to explicitly dispose anything.
                // RestClient recommends simply creating a new instance
                // for each new request, as it handles the web socket on
                // the backend.
            }

            _disposed = true;
        }
    }
}