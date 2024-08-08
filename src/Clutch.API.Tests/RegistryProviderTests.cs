using Clutch.API.Models.Enums;
using Clutch.API.Providers.Registry;
using Clutch.API.Providers.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using RestSharp;
using System.Net;
using Clutch.API.Models.Registry;

namespace Clutch.API.Tests
{
    // TODO:
    // - Add tests for Azure provider
    // - Fix broken tests; failing because multiple ExecuteAsync calls are made
    // but only one variety is Mocked
    [TestClass]
    public class RegistryProviderTests
    {
        private Mock<ILogger> _mockLogger;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<IRestClientFactory> _mockRestClientFactory;

        [TestInitialize]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockRestClientFactory = new Mock<IRestClientFactory>();

            /*
            _mockConfiguration.Setup(c => c.GetSection(It.IsAny<string>()).Value)
                  .Returns("your_test_connection_string");
            _mockConfiguration.Setup(c => c[It.IsAny<string>()]).Returns("dummy-string"); 
            */
        }

        [TestCleanup]
        public void Cleanup()
        {
            _mockRestClientFactory.VerifyAll();
            _mockRestClientFactory.Reset();
        }

        #region RegistryManifestBase

        [TestMethod]
        public async Task GetManifestAsync_Success_Local()
        {
            // Arrange
            var responseContent = JsonConvert.SerializeObject(TestUtils.GetRegistryManifestModel());
            _mockRestClientFactory.Setup(client => client.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RestResponse { StatusCode = HttpStatusCode.OK, Content = responseContent });
            _mockConfiguration.Setup(c => c.GetSection(It.IsAny<string>()).Value)
                  .Returns("your_test_connection_string");

            var provider = new RegistryProviderBase(_mockRestClientFactory.Object, _mockLogger.Object, _mockConfiguration.Object);
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

        [TestMethod]
        public async Task GetManifestAsync_UnsuccessfulResponse_Local()
        {
            // Arrange
            _mockRestClientFactory.Setup(client => client.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RestResponse { StatusCode = HttpStatusCode.NotFound, Content = null });
            _mockConfiguration.Setup(c => c.GetSection(It.IsAny<string>()).Value)
                  .Returns("your_test_connection_string");

            var provider = new RegistryProviderBase(_mockRestClientFactory.Object, _mockLogger.Object, _mockConfiguration.Object);
            var request = TestUtils.GetContainerImageRequest("lvlup-sw/clutchapi", "dev", RegistryType.Local);

            // Act
            var result = await provider.GetManifestAsync(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasValue);
            _mockRestClientFactory.Verify(c => c.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
        }

        #endregion
        #region DockerRegistryProvider

        [TestMethod]
        public async Task GetManifestAsync_Success_Docker()
        {
            // Arrange
            var responseContent = JsonConvert.SerializeObject(TestUtils.GetRegistryManifestModel());
            _mockRestClientFactory.Setup(client => client.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RestResponse { StatusCode = HttpStatusCode.OK, Content = responseContent });
            _mockConfiguration.Setup(c => c.GetSection(It.IsAny<string>()).Value)
                  .Returns("your_test_connection_string");

            var provider = new DockerRegistryProvider(_mockRestClientFactory.Object, _mockLogger.Object, _mockConfiguration.Object);
            var request = TestUtils.GetContainerImageRequest("joedwards32/cs2", "latest", RegistryType.Docker);

            // Act
            var result = await provider.GetManifestAsync(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasValue);
            _mockRestClientFactory.Verify(c => c.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            RegistryManifestModel? response = JsonConvert.DeserializeObject<RegistryManifestModel>(responseContent);
            Assert.IsTrue(response is not null && TestUtils.ManifestsAreEqual(response, result));
        }

        [TestMethod]
        public async Task GetManifestAsync_UnsuccessfulResponse_Docker()
        {
            // Arrange
            _mockRestClientFactory.Setup(client => client.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RestResponse { StatusCode = HttpStatusCode.NotFound, Content = null });
            _mockConfiguration.Setup(c => c.GetSection(It.IsAny<string>()).Value)
                  .Returns("your_test_connection_string");

            var provider = new DockerRegistryProvider(_mockRestClientFactory.Object, _mockLogger.Object, _mockConfiguration.Object);
            var request = TestUtils.GetContainerImageRequest("joedwards32/cs2", "latest", RegistryType.Docker);

            // Act
            var result = await provider.GetManifestAsync(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasValue);
            _mockRestClientFactory.Verify(c => c.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
        }

        #endregion
        #region Exception Handling

        [TestMethod]
        public async Task GetManifestAsync_DeserializationException()
        {
            // Arrange
            _mockRestClientFactory.Setup(client => client.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RestResponse { StatusCode = HttpStatusCode.OK, Content = "INVALID" });
            _mockConfiguration.Setup(c => c.GetSection(It.IsAny<string>()).Value)
                  .Returns("your_test_connection_string");

            var provider = new DockerRegistryProvider(_mockRestClientFactory.Object, _mockLogger.Object, _mockConfiguration.Object);
            var request = TestUtils.GetContainerImageRequest("joedwards32/cs2", "latest", RegistryType.Docker);

            // Act
            var result = await provider.GetManifestAsync(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasValue);
            Assert.IsTrue(TestUtils.ManifestsAreEqual(RegistryManifestModel.Null, result));
            _mockRestClientFactory.Verify(c => c.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
        }

        #endregion
    }
}
