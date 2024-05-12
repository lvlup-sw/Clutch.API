using Clutch.API.Models.Enums;
using Clutch.API.Properties;
using Clutch.API.Providers.Interfaces;
using RestSharp;

namespace Clutch.API.Providers.Registry
{
    public class RestClientFactory(ILogger<RestClientFactory> logger) : IRestClientFactory
    {
        private readonly ILogger<RestClientFactory> _logger = logger;
        private RestClient? restClient;

        public void InstantiateClient(string endpoint) => restClient = new(endpoint);

        public async Task<RestResponse?> ExecuteAsync(RestRequest request)
        {
            if (restClient is null) return default;

            try
            {
                return await restClient.ExecuteAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing REST request.");
                return null;
            }
        }
    }
}
