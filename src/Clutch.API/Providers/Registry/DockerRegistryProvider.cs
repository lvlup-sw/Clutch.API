﻿using RestSharp;
using System.Buffers;

namespace Clutch.API.Providers.Registry
{
    public class DockerRegistryProvider(IRestClientFactory restClientFactory, ArrayPool<byte> arrayPool, ILogger logger, IConfiguration configuration) : RegistryProviderBase(restClientFactory, logger, configuration)
    {
        private readonly ILogger _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private IRestClientFactory _restClientFactory = restClientFactory;
        // Unused; only for private repos
        private readonly string pat = Convert.ToBase64String(Encoding.UTF8.GetBytes(configuration.GetConnectionString("DockerPAT") ?? string.Empty));

        public override async Task<RegistryManifestModel> GetManifestAsync(ContainerImageRequest request)
        {
            string[] parts = request.Repository.Split('/');
            dynamic rawToken = GetToken(parts);

            // Construct the API request
            _restClientFactory.InstantiateClient("https://registry-1.docker.io");
            RestRequest restRequest = new($"/v2/{parts[0]}/{parts[1]}/manifests/{request.Tag}");
            restRequest.Method = Method.Get;
            restRequest.AddHeader("Accept", "application/vnd.docker.distribution.manifest.v2+json");
            restRequest.AddHeader("Authorization", $"Bearer {rawToken.token}");

            // Send the request
            RestResponse? response = await _restClientFactory.ExecuteAsync(restRequest);

            return await ValidateAndDeserializeResponseAsync(response);
        }

        internal async Task<dynamic> GetToken(string[] parts)
        {
            dynamic rawToken;

            // Get the Bearer Token
            _restClientFactory.InstantiateClient("https://auth.docker.io");
            RestRequest authRequest = new($"/token?service=registry.docker.io&scope=repository:{parts[0]}/{parts[1]}:pull");
            authRequest.Method = Method.Get;
            RestResponse? authResponse = await _restClientFactory.ExecuteAsync(authRequest);

            if (authResponse is null)
            {
                _logger.LogError("Failed to retrieve image manifest from registry due to an exception processing the request.");
                return RegistryManifestModel.Null;
            }
            else if (authResponse.StatusCode != HttpStatusCode.OK || authResponse.Content is null)
            {
                _logger.LogError("Failed to retrieve bearer token for registry. StatusCode: {StatusCode}. ErrorMessage: {ErrorMessage}", authResponse.StatusCode, authResponse.ErrorMessage);
                return RegistryManifestModel.Null;
            }

            try
            {
                rawToken = JsonSerializer.Deserialize<dynamic>(authResponse.Content) ?? string.Empty;
                if (rawToken is string) throw new DeserializationException(authResponse, new("Deserialization returned null."));
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to deserialize bearer token. Exception: {ex}", ex.GetBaseException());
                return RegistryManifestModel.Null;
            }

            return rawToken;
        }
    }
}
