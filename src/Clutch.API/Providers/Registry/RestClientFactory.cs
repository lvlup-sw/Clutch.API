using Clutch.API.Providers.Interfaces;
using RestSharp;

namespace Clutch.API.Providers.Registry
{
    public class RestClientFactory(ILogger<RestClientFactory> logger) : IRestClientFactory
    {
        private readonly ILogger<RestClientFactory> _logger = logger;
        private RestClient? _restClient;

        public void InstantiateClient(string endpoint) => _restClient = new(endpoint);

        public async Task<RestResponse?> ExecuteAsync(RestRequest request, CancellationToken cancellationToken = default)
        {
            if (_restClient is null) return default;

            try
            {
                return await _restClient.ExecuteAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing REST request.");
                return null;
            }
        }
    }
}
