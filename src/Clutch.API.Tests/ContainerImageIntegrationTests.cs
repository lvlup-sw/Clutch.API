using Clutch.API.Models.Image;
using Clutch.API.Models.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;

namespace Clutch.API.Tests
{
    [TestClass]
    public class ContainerImageIntegrationTests
    {
        private WebApplicationFactory<Program> _factory;
        private HttpClient _client;

        [TestInitialize]
        public void TestSetup()
        {
            _factory = new WebApplicationFactory<Program>();
            _client = _factory.CreateClient();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        [TestMethod]
        public async Task GetContainerImage_Success()
        {
            // Arrange
            // We need this query: ContainerImage/GetImage?Repository=lvlup-sw%2Fclutchapi&Tag=dev&RegistryType=0
            var request = TestUtils.GetContainerImageRequest("lvlup-sw/clutchapi", "dev", RegistryType.Local);
            var queryString = $"Repository={System.Net.WebUtility.UrlEncode(request.Repository)}&Tag={request.Tag}&RegistryType={(int)request.RegistryType}";

            // Act
            var response = await _client.GetAsync($"/ContainerImage/GetImage?{queryString}");

            // Assert
            response.EnsureSuccessStatusCode(); // Throws if not 200-299
            var content = await response.Content.ReadAsStringAsync();
            var imageResponse = JsonConvert.DeserializeObject<ContainerImageResponse>(content);
            Assert.IsNotNull(imageResponse);
            Assert.IsTrue(imageResponse.Success);
            Assert.IsNotNull(imageResponse.ContainerImage);
            Assert.IsNotNull(imageResponse.RegistryManifest);
        }
    }
}
