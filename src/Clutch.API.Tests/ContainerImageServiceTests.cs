using Clutch.API.Models.Image;
using Clutch.API.Models.Enums;
using Clutch.API.Models.Registry;
using Clutch.API.Properties;
using Clutch.API.Services.Image;
using Clutch.API.Providers.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

// TODO:
// - Update unit tests to include new EventPublisher class
namespace Clutch.API.Tests
{
    [TestClass]
    public class ContainerImageServiceTests
    {
        private Mock<ICacheProvider<ContainerImageModel>> _mockCacheProvider;
        private Mock<IContainerImageProvider> _mockImageProvider;
        private Mock<IRegistryProviderFactory> _mockRegistryProviderFactory;
        private Mock<IEventPublisher> _mockEventPublisher;
        private Mock<ILogger> _mockLogger;
        private IOptions<AppSettings> _settings;
        private ContainerImageService _service;

        [TestInitialize]
        public void Setup()
        {
            _mockCacheProvider = new Mock<ICacheProvider<ContainerImageModel>>();
            _mockImageProvider = new Mock<IContainerImageProvider>();
            _mockRegistryProviderFactory = new Mock<IRegistryProviderFactory>();
            _mockEventPublisher = new Mock<IEventPublisher>();
            _mockLogger = new Mock<ILogger>();
            _settings = Options.Create(new AppSettings
            {
                ProviderRetryCount = 1,
                CacheExpirationTime = 24,
                ProviderRetryInterval = 2,
                ProviderUseExponentialBackoff = true
            });
            _service = new (_mockCacheProvider.Object, 
                            _mockImageProvider.Object, 
                            _mockRegistryProviderFactory.Object,
                            _mockEventPublisher.Object,
                            _mockLogger.Object, _settings);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _mockCacheProvider.VerifyAll();
            _mockCacheProvider.Reset();
            _mockImageProvider.VerifyAll();
            _mockImageProvider.Reset();
            _mockRegistryProviderFactory.VerifyAll();
            _mockRegistryProviderFactory.Reset();
        }

        #region GetImage

        [TestMethod]
        public async Task GetImageAsync_Success1()
        {
            // Arrange
            var version = "1.0.0";
            var request = TestUtils.GetContainerImageRequest("test/image", "latest", RegistryType.Docker);
            var cacheKey = TestUtils.ConstructCacheKey(request, version);
            var expectedImage = TestUtils.GetContainerImageByRequest("test/image", "latest", RegistryType.Docker);
            var expectedManifest = TestUtils.GetRegistryManifestModel();
            ContainerImageResponseData expectedResponse = new(true, expectedImage, expectedManifest);

            var mockRegistryProvider = new Mock<IRegistryProvider>();
            mockRegistryProvider.Setup(r => r.GetManifestAsync(request))
                .Returns(Task.FromResult(expectedManifest));
            _mockRegistryProviderFactory
                .Setup(f => f.CreateRegistryProvider(It.Is<RegistryType>(t => t == RegistryType.Docker)))
                .Returns(mockRegistryProvider.Object);
            _mockCacheProvider.Setup(c => c.GetFromCacheAsync(cacheKey, default))
                .ReturnsAsync(expectedImage);

            // Act
            var result = await _service.GetImageAsync(request, version);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(TestUtils.ResponseDataAreEqual(expectedResponse, result));
        }

        [TestMethod]
        public async Task GetImageAsync_Success2()
        {
            // Arrange
            var version = "1.0.0";
            var request = TestUtils.GetContainerImageRequest("joedward32/cs2", "latest", RegistryType.Docker);
            var cacheKey = TestUtils.ConstructCacheKey(request, version);
            var expectedImage = TestUtils.GetContainerImageByRequest("joedward32/cs2", "latest", RegistryType.Docker);
            var expectedManifest = TestUtils.GetRegistryManifestModel();
            ContainerImageResponseData expectedResponse = new(true, expectedImage, expectedManifest);

            var mockRegistryProvider = new Mock<IRegistryProvider>();
            mockRegistryProvider.Setup(r => r.GetManifestAsync(request))
                .Returns(Task.FromResult(expectedManifest));
            _mockRegistryProviderFactory
                .Setup(f => f.CreateRegistryProvider(It.Is<RegistryType>(t => t == RegistryType.Docker)))
                .Returns(mockRegistryProvider.Object);
            _mockCacheProvider.Setup(c => c.GetFromCacheAsync(cacheKey, default))
                .ReturnsAsync(expectedImage);

            // Act
            var result = await _service.GetImageAsync(request, version);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(TestUtils.ResponseDataAreEqual(expectedResponse, result));
        }

        [TestMethod]
        public async Task GetImageAsync_Success3()
        {
            // Arrange
            var version = "1.0.0";
            var request = TestUtils.GetContainerImageRequest("lvlup-sw/clutchapi", "dev", RegistryType.Local);
            var cacheKey = TestUtils.ConstructCacheKey(request, version);
            var expectedImage = TestUtils.GetContainerImageByRequest("lvlup-sw/clutchapi", "dev", RegistryType.Local);
            var expectedManifest = TestUtils.GetRegistryManifestModel();
            ContainerImageResponseData expectedResponse = new(true, expectedImage, expectedManifest);

            var mockRegistryProvider = new Mock<IRegistryProvider>();
            mockRegistryProvider.Setup(r => r.GetManifestAsync(request))
                .Returns(Task.FromResult(expectedManifest));
            _mockRegistryProviderFactory
                .Setup(f => f.CreateRegistryProvider(It.Is<RegistryType>(t => t == RegistryType.Local)))
                .Returns(mockRegistryProvider.Object);
            _mockCacheProvider.Setup(c => c.GetFromCacheAsync(cacheKey, default))
                .ReturnsAsync(expectedImage);

            // Act
            var result = await _service.GetImageAsync(request, version);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(TestUtils.ResponseDataAreEqual(expectedResponse, result));
        }

        [TestMethod]
        public async Task GetImageAsync_ImageNotFound1()
        {
            // Arrange
            var version = "1.0.0";
            var request = TestUtils.GetContainerImageRequest("test/image", "latest", RegistryType.Docker);
            var cacheKey = TestUtils.ConstructCacheKey(request, version);
            ContainerImageResponseData expectedResponse = new(false, ContainerImageModel.Null, RegistryManifestModel.Null);

            _mockCacheProvider.Setup(c => c.GetFromCacheAsync(cacheKey, default))
                .ReturnsAsync(default(ContainerImageModel));

            // Act
            var result = await _service.GetImageAsync(request, version);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(TestUtils.ResponseDataAreEqual(expectedResponse, result));
        }

        [TestMethod]
        public async Task GetImageAsync_ImageNotFound2()
        {
            // Arrange
            var version = "1.0.0";
            var request = TestUtils.GetContainerImageRequest("joedward32/cs2", "latest", RegistryType.Docker);
            var cacheKey = TestUtils.ConstructCacheKey(request, version);
            ContainerImageResponseData expectedResponse = new(false, ContainerImageModel.Null, RegistryManifestModel.Null);

            _mockCacheProvider.Setup(c => c.GetFromCacheAsync(cacheKey, default))
                .ReturnsAsync(default(ContainerImageModel));

            // Act
            var result = await _service.GetImageAsync(request, version);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(TestUtils.ResponseDataAreEqual(expectedResponse, result));
        }

        [TestMethod]
        public async Task GetImageAsync_ImageNotFound3()
        {
            // Arrange
            var version = "1.0.0";
            var request = TestUtils.GetContainerImageRequest("lvlup-sw/clutchapi", "dev", RegistryType.Local);
            var cacheKey = TestUtils.ConstructCacheKey(request, version);
            ContainerImageResponseData expectedResponse = new(false, ContainerImageModel.Null, RegistryManifestModel.Null);

            _mockCacheProvider.Setup(c => c.GetFromCacheAsync(cacheKey, default))
                .ReturnsAsync(default(ContainerImageModel));

            // Act
            var result = await _service.GetImageAsync(request, version);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(TestUtils.ResponseDataAreEqual(expectedResponse, result));
        }

        [TestMethod]
        public async Task GetImageAsync_ManifestNotFound1()
        {
            // Arrange
            var version = "1.0.0";
            var request = TestUtils.GetContainerImageRequest("test/image", "latest", RegistryType.Docker);
            var cacheKey = TestUtils.ConstructCacheKey(request, version);
            var expectedImage = TestUtils.GetContainerImageByRequest("test/image", "latest", RegistryType.Docker);
            ContainerImageResponseData expectedResponse = new(false, ContainerImageModel.Null, RegistryManifestModel.Null);

            var mockRegistryProvider = new Mock<IRegistryProvider>();
            mockRegistryProvider.Setup(r => r.GetManifestAsync(request))
                .Returns(Task.FromResult(RegistryManifestModel.Null));
            _mockRegistryProviderFactory
                .Setup(f => f.CreateRegistryProvider(It.Is<RegistryType>(t => t == RegistryType.Docker)))
                .Returns(mockRegistryProvider.Object);
            _mockCacheProvider.Setup(c => c.GetFromCacheAsync(cacheKey, default))
                .ReturnsAsync(expectedImage);

            // Act
            var result = await _service.GetImageAsync(request, version);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(TestUtils.ResponseDataAreEqual(expectedResponse, result));
        }

        [TestMethod]
        public async Task GetImageAsync_ManifestNotFound2()
        {
            // Arrange
            var version = "1.0.0";
            var request = TestUtils.GetContainerImageRequest("joedward32/cs2", "latest", RegistryType.Docker);
            var cacheKey = TestUtils.ConstructCacheKey(request, version);
            var expectedImage = TestUtils.GetContainerImageByRequest("joedward32/cs2", "latest", RegistryType.Docker);
            ContainerImageResponseData expectedResponse = new(false, ContainerImageModel.Null, RegistryManifestModel.Null);

            var mockRegistryProvider = new Mock<IRegistryProvider>();
            mockRegistryProvider.Setup(r => r.GetManifestAsync(request))
                .Returns(Task.FromResult(RegistryManifestModel.Null));
            _mockRegistryProviderFactory
                .Setup(f => f.CreateRegistryProvider(It.Is<RegistryType>(t => t == RegistryType.Docker)))
                .Returns(mockRegistryProvider.Object);
            _mockCacheProvider.Setup(c => c.GetFromCacheAsync(cacheKey, default))
                .ReturnsAsync(expectedImage);

            // Act
            var result = await _service.GetImageAsync(request, version);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(TestUtils.ResponseDataAreEqual(expectedResponse, result));
        }

        [TestMethod]
        public async Task GetImageAsync_ManifestNotFound3()
        {
            // Arrange
            var version = "1.0.0";
            var request = TestUtils.GetContainerImageRequest("lvlup-sw/clutchapi", "dev", RegistryType.Local);
            var cacheKey = TestUtils.ConstructCacheKey(request, version);
            var expectedImage = TestUtils.GetContainerImageByRequest("lvlup-sw/clutchapi", "dev", RegistryType.Local);
            ContainerImageResponseData expectedResponse = new(false, ContainerImageModel.Null, RegistryManifestModel.Null);

            var mockRegistryProvider = new Mock<IRegistryProvider>();
            mockRegistryProvider.Setup(r => r.GetManifestAsync(request))
                .Returns(Task.FromResult(RegistryManifestModel.Null));
            _mockRegistryProviderFactory
                .Setup(f => f.CreateRegistryProvider(It.Is<RegistryType>(t => t == RegistryType.Local)))
                .Returns(mockRegistryProvider.Object);
            _mockCacheProvider.Setup(c => c.GetFromCacheAsync(cacheKey, default))
                .ReturnsAsync(expectedImage);

            // Act
            var result = await _service.GetImageAsync(request, version);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(TestUtils.ResponseDataAreEqual(expectedResponse, result));
        }

        #endregion
        #region SetImage

        [TestMethod]
        public async Task SetImageAsync_Success1()
        {
            // Arrange
            var version = "1.0.0";
            var request = TestUtils.GetContainerImageRequest("test/image", "latest", RegistryType.Docker);
            var cacheKey = TestUtils.ConstructCacheKey(request, version);

            _mockCacheProvider
                .Setup(c => c.SetInCacheAsync(cacheKey, It.IsAny<ContainerImageModel>(), default))
                .ReturnsAsync(true);

            // Act
            var result = await _service.SetImageAsync(request, version);

            // Assert
            Assert.IsTrue(result);
            _mockCacheProvider.Verify(p => p.SetInCacheAsync(cacheKey, It.IsAny<ContainerImageModel>(), default), Times.Exactly(1));
        }

        [TestMethod]
        public async Task SetImageAsync_Success2()
        {
            // Arrange
            var version = "1.0.0";
            var request = TestUtils.GetContainerImageRequest("joedward32/cs2", "latest", RegistryType.Docker);
            var cacheKey = TestUtils.ConstructCacheKey(request, version);

            _mockCacheProvider
                .Setup(c => c.SetInCacheAsync(cacheKey, It.IsAny<ContainerImageModel>(), default))
                .ReturnsAsync(true);

            // Act
            var result = await _service.SetImageAsync(request, version);

            // Assert
            Assert.IsTrue(result);
            _mockCacheProvider.Verify(p => p.SetInCacheAsync(cacheKey, It.IsAny<ContainerImageModel>(), default), Times.Exactly(1));
        }

        [TestMethod]
        public async Task SetImageAsync_Success3()
        {
            // Arrange
            var version = "1.0.0";
            var request = TestUtils.GetContainerImageRequest("lvlup-sw/clutchapi", "dev", RegistryType.Local);
            var cacheKey = TestUtils.ConstructCacheKey(request, version);

            _mockCacheProvider
                .Setup(c => c.SetInCacheAsync(cacheKey, It.IsAny<ContainerImageModel>(), default))
                .ReturnsAsync(true);

            // Act
            var result = await _service.SetImageAsync(request, version);

            // Assert
            Assert.IsTrue(result);
            _mockCacheProvider.Verify(p => p.SetInCacheAsync(cacheKey, It.IsAny<ContainerImageModel>(), default), Times.Exactly(1));
        }

        [TestMethod]
        public async Task SetImageAsync_Failure1()
        {
            // Arrange
            var version = "1.0.0";
            var request = TestUtils.GetContainerImageRequest("test/image", "latest", RegistryType.Docker);
            var cacheKey = TestUtils.ConstructCacheKey(request, version);

            _mockCacheProvider
                .Setup(c => c.SetInCacheAsync(cacheKey, It.IsAny<ContainerImageModel>(), default))
                .ReturnsAsync(false);

            // Act
            var result = await _service.SetImageAsync(request, version);

            // Assert
            Assert.IsFalse(result);
            _mockCacheProvider.Verify(p => p.SetInCacheAsync(cacheKey, It.IsAny<ContainerImageModel>(), default), Times.Exactly(1));
        }

        [TestMethod]
        public async Task SetImageAsync_Failure2()
        {
            // Arrange
            var version = "1.0.0";
            var request = TestUtils.GetContainerImageRequest("joedward32/cs2", "latest", RegistryType.Docker);
            var cacheKey = TestUtils.ConstructCacheKey(request, version);

            _mockCacheProvider
                .Setup(c => c.SetInCacheAsync(cacheKey, It.IsAny<ContainerImageModel>(), default))
                .ReturnsAsync(false);

            // Act
            var result = await _service.SetImageAsync(request, version);

            // Assert
            Assert.IsFalse(result);
            _mockCacheProvider.Verify(p => p.SetInCacheAsync(cacheKey, It.IsAny<ContainerImageModel>(), default), Times.Exactly(1));
        }

        [TestMethod]
        public async Task SetImageAsync_Failure3()
        {
            // Arrange
            var version = "1.0.0";
            var request = TestUtils.GetContainerImageRequest("lvlup-sw/clutchapi", "dev", RegistryType.Local);
            var cacheKey = TestUtils.ConstructCacheKey(request, version);

            _mockCacheProvider
                .Setup(c => c.SetInCacheAsync(cacheKey, It.IsAny<ContainerImageModel>(), default))
                .ReturnsAsync(false);

            // Act
            var result = await _service.SetImageAsync(request, version);

            // Assert
            Assert.IsFalse(result);
            _mockCacheProvider.Verify(p => p.SetInCacheAsync(cacheKey, It.IsAny<ContainerImageModel>(), default), Times.Exactly(1));
        }

        #endregion
        #region DeleteImage

        [TestMethod]
        public async Task DeleteImageAsync_Success1()
        {
            // Arrange
            var version = "1.0.0";
            var request = TestUtils.GetContainerImageRequest("test/image", "latest", RegistryType.Docker);
            var cacheKey = TestUtils.ConstructCacheKey(request, version);

            _mockCacheProvider
                .Setup(c => c.RemoveFromCacheAsync(cacheKey))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteImageAsync(request, version);

            // Assert
            Assert.IsTrue(result);
            _mockCacheProvider.Verify(p => p.RemoveFromCacheAsync(cacheKey), Times.Exactly(1));
        }

        [TestMethod]
        public async Task DeleteImageAsync_Success2()
        {
            // Arrange
            var version = "1.0.0";
            var request = TestUtils.GetContainerImageRequest("joedward32/cs2", "latest", RegistryType.Docker);
            var cacheKey = TestUtils.ConstructCacheKey(request, version);

            _mockCacheProvider
                .Setup(c => c.RemoveFromCacheAsync(cacheKey))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteImageAsync(request, version);

            // Assert
            Assert.IsTrue(result);
            _mockCacheProvider.Verify(p => p.RemoveFromCacheAsync(cacheKey), Times.Exactly(1));
        }

        [TestMethod]
        public async Task DeleteImageAsync_Success3()
        {
            // Arrange
            var version = "1.0.0";
            var request = TestUtils.GetContainerImageRequest("lvlup-sw/clutchapi", "dev", RegistryType.Local);
            var cacheKey = TestUtils.ConstructCacheKey(request, version);

            _mockCacheProvider
                .Setup(c => c.RemoveFromCacheAsync(cacheKey))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteImageAsync(request, version);

            // Assert
            Assert.IsTrue(result);
            _mockCacheProvider.Verify(p => p.RemoveFromCacheAsync(cacheKey), Times.Exactly(1));
        }

        [TestMethod]
        public async Task DeleteImageAsync_Failure1()
        {
            // Arrange
            var version = "1.0.0";
            var request = TestUtils.GetContainerImageRequest("test/image", "latest", RegistryType.Docker);
            var cacheKey = TestUtils.ConstructCacheKey(request, version);

            _mockCacheProvider
                .Setup(c => c.RemoveFromCacheAsync(cacheKey))
                .ReturnsAsync(false);

            // Act
            var result = await _service.DeleteImageAsync(request, version);

            // Assert
            Assert.IsFalse(result);
            _mockCacheProvider.Verify(p => p.RemoveFromCacheAsync(cacheKey), Times.Exactly(1));
        }

        [TestMethod]
        public async Task DeleteImageAsync_Failure2()
        {
            // Arrange
            var version = "1.0.0";
            var request = TestUtils.GetContainerImageRequest("joedward32/cs2", "latest", RegistryType.Docker);
            var cacheKey = TestUtils.ConstructCacheKey(request, version);

            _mockCacheProvider
                .Setup(c => c.RemoveFromCacheAsync(cacheKey))
                .ReturnsAsync(false);

            // Act
            var result = await _service.DeleteImageAsync(request, version);

            // Assert
            Assert.IsFalse(result);
            _mockCacheProvider.Verify(p => p.RemoveFromCacheAsync(cacheKey), Times.Exactly(1));
        }

        [TestMethod]
        public async Task DeleteImageAsync_Failure3()
        {
            // Arrange
            var version = "1.0.0";
            var request = TestUtils.GetContainerImageRequest("lvlup-sw/clutchapi", "dev", RegistryType.Local);
            var cacheKey = TestUtils.ConstructCacheKey(request, version);

            _mockCacheProvider
                .Setup(c => c.RemoveFromCacheAsync(cacheKey))
                .ReturnsAsync(false);

            // Act
            var result = await _service.DeleteImageAsync(request, version);

            // Assert
            Assert.IsFalse(result);
            _mockCacheProvider.Verify(p => p.RemoveFromCacheAsync(cacheKey), Times.Exactly(1));
        }

        #endregion
    }
}
