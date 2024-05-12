using RestSharp;

namespace Clutch.API.Providers.Interfaces
{
    public interface IRestClientFactory
    {
        void InstantiateClient(string endpoint);
        Task<RestResponse?> ExecuteAsync(RestRequest request);
    }
}
