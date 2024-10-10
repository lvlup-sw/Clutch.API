using Clutch.API.Providers.Image;
using Clutch.API.Models.Image;
using Clutch.API.Models.Enums;
using Clutch.API.Repositories.Interfaces;
using Clutch.API.Properties;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Clutch.API.Tests
{
    [TestClass]
    public class ContainerImageProviderTests
    {
        private Mock<IContainerImageRepository> _mockRepository;
        private Mock<ILogger> _mockLogger;
        private IOptions<AppSettings> _settings;
        private ContainerImageProvider _provider;

        [TestInitialize]
        public void Setup()
        {
            _mockRepository = new Mock<IContainerImageRepository>();
            _mockLogger = new Mock<ILogger>();
            _settings = Options.Create(new AppSettings
            {
                ProviderRetryCount = 1,
                CacheExpirationTime = 24,
                ProviderRetryInterval = 2,
                ProviderUseExponentialBackoff = true
            });
            _provider = new ContainerImageProvider(_mockRepository.Object, _mockLogger.Object, _settings);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _mockRepository.VerifyAll();
            _mockRepository.Reset();
        }

        #region GetImage

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(5)]
        [DataRow(10)]
        [TestMethod]
        public async Task GetImageByIdAsync_Success(int testImageId)
        {
            // Arrange
            var expectedImage = TestUtils.GetContainerImage(testImageId);
            _mockRepository.Setup(repo => repo.GetImageAsync(It.Is<int>(id => id == testImageId)))
                .ReturnsAsync(expectedImage);

            // Act
            var result = await _provider.GetImageAsync(testImageId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasValue);
            Assert.IsTrue(TestUtils.ImagesAreEqual(expectedImage, result));
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(6)]
        [DataRow(99)]
        [TestMethod]
        public async Task GetImageByIdAsync_NotFound(int nonExistentImageId)
        {
            // Arrange
            var expectedImage = TestUtils.GetContainerImage(nonExistentImageId);
            _mockRepository.Setup(repo => repo.GetImageAsync(It.IsAny<int>()))!
                .ReturnsAsync(default(ContainerImageModel));

            // Act
            var result = await _provider.GetImageAsync(nonExistentImageId);

            // Assert
            Assert.IsNull(result);
            _mockRepository.Verify(repo => repo.GetImageAsync(nonExistentImageId), Times.Exactly(2));
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        [TestMethod]
        public async Task GetImageByIdAsync_NotFoundInvalid(int nonExistentImageId)
        {
            // Act & Arrange
            var result = await _provider.DeleteFromSourceAsync(nonExistentImageId.ToString());

            // Assert
            Assert.IsFalse(result);
            _mockRepository.Verify(repo => repo.DeleteImageAsync(nonExistentImageId), Times.Never);
        }

        [DataTestMethod]
        [DataRow("test/image:latest")]
        [DataRow("joedward32/cs2:latest")]
        [DataRow("lvlup-sw/clutchapi:dev")]
        [TestMethod]
        public async Task GetImageByReferenceAsync_Success(string repositoryId)
        {
            // Arrange
            var expectedImage = TestUtils.GetContainerImageByReference(repositoryId);
            _mockRepository.Setup(repo => repo.GetImageAsync(It.Is<string>(id => id.Equals(repositoryId))))
                .ReturnsAsync(expectedImage);

            // Act
            var result = await _provider.GetImageAsync($"1.0.0:{repositoryId}:testhash");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasValue);
            Assert.IsTrue(TestUtils.ImagesAreEqual(expectedImage, result));
        }

        [DataTestMethod]
        [DataRow("test/image:latest")]
        [DataRow("joedward32/cs2:latest")]
        [DataRow("lvlup-sw/clutchapi:dev")]
        [TestMethod]
        public async Task GetImageByReferenceAsync_NotFound(string repositoryId)
        {
            // Arrange
            var expectedImage = TestUtils.GetContainerImageByReference(repositoryId);
            _mockRepository.Setup(repo => repo.GetImageAsync(It.IsAny<string>()))!
                .ReturnsAsync(default(ContainerImageModel));

            // Act
            var result = await _provider.GetImageAsync($"1.0.0:{repositoryId}:testhash");

            // Assert
            Assert.IsNull(result);
            _mockRepository.Verify(repo => repo.GetImageAsync(repositoryId), Times.Exactly(2));
        }

        [DataTestMethod]
        [DataRow(default)]
        [DataRow(null)]
        [DataRow("lvlup-sw/clutchapi")]
        [TestMethod]
        public async Task GetImageByReferenceAsync_NotFoundInvalid(string repositoryId)
        {
            // Act & Arrange
            var result = await _provider.GetImageAsync(repositoryId);

            // Assert
            Assert.IsNull(result);
            _mockRepository.Verify(repo => repo.GetImageAsync(repositoryId), Times.Never);
        }

        #endregion
        #region SetImage

        // DataRow doesn't allow for dynamically created data structures
        [TestMethod]
        public async Task SetImageAsync_Success1()
        {
            // Arrange
            var request = TestUtils.GetContainerImageByRequest("test/image", "latest", RegistryType.Docker);
            _mockRepository.Setup(repo => repo.SetImageAsync(It.IsAny<ContainerImageModel>()))
                .Returns(Task.FromResult(true));

            // Act
            var result = await _provider.SetImageAsync(request);

            // Assert
            Assert.IsTrue(result);
            _mockRepository.Verify(repo => repo.SetImageAsync(request), Times.Once);
        }

        [TestMethod]
        public async Task SetImageAsync_Success2()
        {
            // Arrange
            var request = TestUtils.GetContainerImageByRequest("joedwards32/cs2", "latest", RegistryType.Docker);
            _mockRepository.Setup(repo => repo.SetImageAsync(It.IsAny<ContainerImageModel>()))
                .Returns(Task.FromResult(true));

            // Act
            var result = await _provider.SetImageAsync(request);

            // Assert
            Assert.IsTrue(result);
            _mockRepository.Verify(repo => repo.SetImageAsync(request), Times.Once);
        }

        [TestMethod]
        public async Task SetImageAsync_Success3()
        {
            // Arrange
            var request = TestUtils.GetContainerImageByRequest("lvlup-sw/clutchapi", "dev", RegistryType.Local);
            _mockRepository.Setup(repo => repo.SetImageAsync(It.IsAny<ContainerImageModel>()))
                .Returns(Task.FromResult(true));

            // Act
            var result = await _provider.SetImageAsync(request);

            // Assert
            Assert.IsTrue(result);
            _mockRepository.Verify(repo => repo.SetImageAsync(request), Times.Once);
        }

        [TestMethod]
        public async Task SetImageAsync_Failure1_WithInvalid()
        {
            // Arrange
            var request = TestUtils.GetContainerImageByRequest("", "", RegistryType.Invalid);

            // Act
            var result = await _provider.SetImageAsync(request);

            // Assert
            Assert.IsFalse(result);
            _mockRepository.Verify(repo => repo.SetImageAsync(request), Times.Never);
        }

        [TestMethod]
        public async Task SetImageAsync_Failure2_WithNull()
        {
            // Act & Arrange
            var result = await _provider.SetImageAsync(ContainerImageModel.Null);

            // Assert
            Assert.IsFalse(result);
            _mockRepository.Verify(repo => repo.SetImageAsync(ContainerImageModel.Null), Times.Never);
        }

        #endregion
        #region DeleteImage

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(5)]
        [DataRow(10)]
        [TestMethod]
        public async Task DeleteImageByIdAsync_Success(int testImageId)
        {
            // Arrange
            _mockRepository.Setup(repo => repo.DeleteImageAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(true));

            // Act
            var result = await _provider.DeleteFromSourceAsync(testImageId.ToString());

            // Assert
            Assert.IsTrue(result);
            _mockRepository.Verify(repo => repo.DeleteImageAsync(testImageId), Times.Once);
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(6)]
        [DataRow(99)]
        [TestMethod]
        public async Task DeleteImageByIdAsync_NotFound(int testImageId)
        {
            // Arrange
            _mockRepository.Setup(repo => repo.DeleteImageAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(false));

            // Act
            var result = await _provider.DeleteFromSourceAsync(testImageId.ToString());

            // Assert
            Assert.IsFalse(result);
            _mockRepository.Verify(repo => repo.DeleteImageAsync(testImageId), Times.Once);
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        [TestMethod]
        public async Task DeleteImageByIdAsync_NotFoundInvalid(int testImageId)
        {
            // Act & Arrange
            var result = await _provider.DeleteFromSourceAsync(testImageId.ToString());

            // Assert
            Assert.IsFalse(result);
            _mockRepository.Verify(repo => repo.DeleteImageAsync(testImageId), Times.Never);
        }

        [DataTestMethod]
        [DataRow("test/image:latest")]
        [DataRow("joedward32/cs2:latest")]
        [DataRow("lvlup-sw/clutchapi:dev")]
        [TestMethod]
        public async Task DeleteImageByReferenceAsync_Success(string repositoryId)
        {
            // Arrange
            _mockRepository.Setup(repo => repo.DeleteImageAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(true));

            // Act
            var result = await _provider.DeleteFromSourceAsync($"1.0.0:{repositoryId}:testhash");

            // Assert
            Assert.IsTrue(result);
            _mockRepository.Verify(repo => repo.DeleteImageAsync(repositoryId), Times.Once);
        }

        [DataTestMethod]
        [DataRow("test/image:latest")]
        [DataRow("joedward32/cs2:latest")]
        [DataRow("lvlup-sw/clutchapi:dev")]
        [TestMethod]
        public async Task DeleteImageByReferenceAsync_NotFound(string repositoryId)
        {
            // Arrange
            _mockRepository.Setup(repo => repo.DeleteImageAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(false));

            // Act
            var result = await _provider.DeleteFromSourceAsync($"1.0.0:{repositoryId}:testhash");

            // Assert
            Assert.IsFalse(result);
            _mockRepository.Verify(repo => repo.DeleteImageAsync(repositoryId), Times.Once);
        }

        [DataTestMethod]
        [DataRow(default)]
        [DataRow(null)]
        [DataRow("lvlup-sw/clutchapi")]
        [TestMethod]
        public async Task DeleteImageByReferenceAsync_NotFoundInvalid(string repositoryId)
        {
            // Act & Arrange
            var result = await _provider.DeleteFromSourceAsync(repositoryId);

            // Assert
            Assert.IsFalse(result);
            _mockRepository.Verify(repo => repo.DeleteImageAsync(repositoryId), Times.Never);
        }

        #endregion
        #region Special Cases


        [TestMethod]
        public async Task GetImageAsync_RepositoryThrowsException_ReturnsNull()
        {
            // Arrange
            var exception = new Exception("Simulated repository error");
            _mockRepository.Setup(repo => repo.GetImageAsync(It.IsAny<int>()))
                           .ThrowsAsync(exception);

            // Act
            var result = await _provider.GetImageAsync(123);

            // Assert
            Assert.IsNull(result);
            _mockRepository.Verify(repo => repo.GetImageAsync(123), Times.Exactly(2));
        }

        [TestMethod]
        public async Task GetImageAsync_TransientError_RetriesCorrectly()
        {
            // Arrange
            var expectedImage = TestUtils.GetContainerImage(1);
            int retryCount = 0;
            _mockRepository
                .Setup(repo => repo.GetImageAsync(It.IsAny<int>()))
                .ReturnsAsync(() =>
                {
                    if (retryCount < 1)
                    {
                        retryCount++;
                        throw new Exception("Transient error");
                    }
                    return expectedImage;
                });

            // Act
            var result = await _provider.GetImageAsync(1);

            // Assert
            Assert.AreEqual(expectedImage, result);
            _mockRepository.Verify(repo => repo.GetImageAsync(1), Times.Exactly(2));
        }

        [TestMethod]
        public async Task GetImageAsync_TransientError_UsesFallbackCorrectly()
        {
            // Arrange
            var expectedImage = TestUtils.GetContainerImage(1);
            int retryCount = 0;
            _mockRepository
                .Setup(repo => repo.GetImageAsync(It.IsAny<int>()))
                .ReturnsAsync(() =>
                {
                    if (retryCount < 1)
                    {
                        retryCount++;
                        throw new Exception("Transient error");
                    }
                    throw new Exception("Final transient error");
                });

            // Act
            var result = await _provider.GetImageAsync(1);

            // Assert
            Assert.IsNull(result);
            _mockRepository.Verify(repo => repo.GetImageAsync(1), Times.Exactly(2));
        }

        #endregion
    }
}
