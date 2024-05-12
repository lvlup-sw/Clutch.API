using Clutch.API.Models.Enums;
using Clutch.API.Properties;
using Clutch.API.Providers.Registry;
using Clutch.API.Providers.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using RestSharp;
using System.Net;
using Clutch.API.Models.Registry;

namespace Clutch.API.Tests
{
    [TestClass]
    public class RegistryProviderTests
    {
        private Mock<ILogger> _mockLogger;
        private IOptions<AppSettings> _mockSettings;
        private Mock<IRestClientFactory> _mockRestClientFactory;

        [TestInitialize]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger>();
            _mockSettings = Options.Create(new AppSettings { GithubPAT = "fake_pat", DockerPAT = "fake_pat" });
            _mockRestClientFactory = new Mock<IRestClientFactory>();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _mockRestClientFactory.VerifyAll();
            _mockRestClientFactory.Reset();
        }

        #region RegistryManifestBase

        [TestMethod]
        public async Task GetManifestAsync_Success()
        {
            // Arrange
            var responseContent = JsonConvert.SerializeObject(TestUtils.GetRegistryManifestModel());
            _mockRestClientFactory.Setup(client => client.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RestResponse { StatusCode = HttpStatusCode.OK, Content = responseContent });

            var provider = new RegistryProviderBase(_mockRestClientFactory.Object, _mockLogger.Object, _mockSettings);
            var request = TestUtils.GetContainerImageRequest("lvlup-sw/clutchapi", "dev", RegistryType.Local);

            // Act
            var result = await provider.GetManifestAsync(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasValue);
            _mockRestClientFactory.Verify(c => c.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
            RegistryManifestModel? response = JsonConvert.DeserializeObject<RegistryManifestModel>(responseContent);
            Assert.IsTrue(response is not null && TestUtils.ManifestsAreEqual(response, result));
        }

        /*
        [TestMethod]
        public async Task GetManifestAsync_UnsuccessfulResponse()
        {
            // Arrange
            var mockRestClient = new Mock<RestClient>();
            mockRestClient.Setup(client => client.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new RestResponse { IsSuccessful = false, ErrorMessage = "Some error" });

            var provider = new RegistryProviderBase(_mockLogger.Object, _mockSettings, mockRestClient.Object);
            var request = new ContainerImageRequest { Repository = "owner/repo", Tag = "latest" };

            // Act
            var result = await provider.GetManifestAsync(request);

            // Assert
            Assert.AreEqual(RegistryManifestModel.Null, result); // Null object for failure
        }
        */

        #endregion
        #region DockerRegistryProvider

        /*
        [TestMethod]
        public async Task GetManifestAsync_Docker_Success()
        {
            // Arrange
            var mockRestClient = new Mock<RestClient>();
            var mockAuthClient = new Mock<RestClient>();

            var tokenResponse = new { token = "fake_token" };
            var manifestContent = JsonConvert.SerializeObject(new RegistryManifestModel());

            mockAuthClient.Setup(client => client.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new RestResponse { IsSuccessful = true, Content = JsonConvert.SerializeObject(tokenResponse) });

            mockRestClient.Setup(client => client.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new RestResponse { IsSuccessful = true, Content = manifestContent });

            var provider = new DockerRegistryProvider(_mockLogger.Object, _mockSettings, mockRestClient.Object, mockAuthClient.Object);
            var request = new ContainerImageRequest { Repository = "library/nginx", Tag = "latest" };

            // Act
            var result = await provider.GetManifestAsync(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasValue);
        }

        [TestMethod]
        public async Task GetManifestAsync_Docker_UnsuccessfulResponse()
        {
            // Arrange
            var mockRestClient = new Mock<RestClient>();
            mockRestClient.Setup(client => client.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new RestResponse { IsSuccessful = false, ErrorMessage = "Some error" });

            var provider = new RegistryProviderBase(_mockLogger.Object, _mockSettings, mockRestClient.Object);
            var request = new ContainerImageRequest { Repository = "owner/repo", Tag = "latest" };

            // Act
            var result = await provider.GetManifestAsync(request);

            // Assert
            Assert.AreEqual(RegistryManifestModel.Null, result); // Null object for failure
        }
        */

        #endregion
        #region Exception Handling

        /*  Exception Handling:
            Unsuccessful token retrieval
            Unsuccessful manifest retrieval
            Invalid JSON response for token
            Invalid JSON response for manifest
        */

        #endregion
    }
}
