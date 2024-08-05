using RestSharp;

namespace Clutch.API.Providers.Registry
{
    // This is essentially a wrapper class managing access to RestClient objects.
    // We primarily need this to allow for mocking in unit tests, since RestClient
    // recommends treating its objects as reusable resources. Otherwise, we'd have
    // to inject numerous instances of RestClient instances into our providers.
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
