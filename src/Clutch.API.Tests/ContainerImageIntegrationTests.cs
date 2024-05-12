using Clutch.API.Models.Image;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;

namespace Clutch.API.Tests
{
    [TestClass]
    public class ContainerImageIntegrationTests
    {
        private static WebApplicationFactory<Program> _factory;
        private static HttpClient _client;

        [ClassInitialize]
        public static void ClassSetup(TestContext context)
        {
            _factory = new WebApplicationFactory<Program>();
        }

        [TestInitialize]
        public void TestSetup()
        {
            _client = _factory.CreateClient();
        }

        [ClassCleanup]
        public static void ClassCleanup() => _factory.Dispose();

        [TestCleanup]
        public static void TestCleanup() => _client.Dispose();

        /*
        [TestMethod]
        public async Task GetContainerImage_Success()
        {
            // Arrange
            var request = new ContainerImageRequest { };
            var queryString = $"?repository={request.Repository}&tag={request.Tag}&registryType={(int)request.RegistryType}";

            // Act
            var response = await _client.GetAsync($"/ContainerImage/GetImage/{queryString}");
            response.EnsureSuccessStatusCode(); // Throws if not 200-299

            // Assert
            var content = await response.Content.ReadAsStringAsync();
            var imageResponse = JsonConvert.DeserializeObject<ContainerImageResponse>(content);
            // ... (your assertions for the response data)
        }
        */
    }
}
