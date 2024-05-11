using Clutch.API.Models.Image;
using Clutch.API.Models.Enums;
using Clutch.API.Models.Registry;
using Clutch.API.Properties;
using Clutch.API.Services.Image;
using Clutch.API.Providers.Interfaces;
using CacheProvider.Providers.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Clutch.API.Tests
{
    [TestClass]
    public class ContainerImageServiceTests
    {
        private Mock<ICacheProvider<ContainerImageModel>> _mockCacheProvider;
        private Mock<IContainerImageProvider> _mockImageProvider;
        private Mock<IRegistryProviderFactory> _mockRegistryProviderFactory;
        private Mock<ILogger> _mockLogger;
        private IOptions<AppSettings> _settings;
        private ContainerImageService _service;

        [TestInitialize]
        public void Setup()
        {
            _mockCacheProvider = new Mock<ICacheProvider<ContainerImageModel>>();
            _mockImageProvider = new Mock<IContainerImageProvider>();
            _mockRegistryProviderFactory = new Mock<IRegistryProviderFactory>();
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


        [TestMethod]
        public async Task GetImageAsync_Success1()
        {
            /*
            // Arrange
            var request = TestUtils.GetContainerImageRequest("lvlup-sw/clutchapi", "dev", RegistryType.Local);                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        
            var version = "1.0.0";
            var cacheKey = TestUtils.ConstructCacheKey(request, version);
            var expectedImage = TestUtils.GetContainerImageByRequest("lvlup-sw/clutchapi", "dev", RegistryType.Local);
            var expectedManifest = new RegistryManifestModel { };
                                                                     
            _mockCacheProvider.Setup(c => c.GetFromCacheAsync(cacheKey)).ReturnsAsync(expectedImage);

            var mockRegistryProvider = new Mock<IRegistryProvider>();
            mockRegistryProvider.Setup(r => r.GetManifestAsync(request)).ReturnsAsync(expectedManifest);
            _mockRegistryProviderFactory.Setup(f => f.CreateRegistryProvider(It.IsAny<RegistryType>()))
                                       .Returns(mockRegistryProvider.Object);

            // Act
            var result = await _service.GetImageAsync(request, version);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(expectedImage, result.Image);
            Assert.AreEqual(expectedManifest, result.Manifest);
            */
        }
    }
}
