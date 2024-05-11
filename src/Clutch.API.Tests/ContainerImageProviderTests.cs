using Clutch.API.Providers.Image;
using Clutch.API.Models.Image;
using Clutch.API.Repositories.Interfaces;
using Clutch.API.Properties;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Clutch.API.Models.Enums;
using NuGet.Protocol.Core.Types;

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
            Assert.IsTrue(result is not null);
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
            Assert.IsTrue(result is null);
            _mockRepository.Verify(repo => repo.GetImageAsync(nonExistentImageId), Times.Once);
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        [TestMethod]
        public async Task GetImageByIdAsync_NotFoundInvalid(int nonExistentImageId)
        {
            // Act & Arrange
            var result = await _provider.DeleteFromDatabaseAsync(nonExistentImageId);

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
            var result = await _provider.GetImageAsync(repositoryId);

            // Assert
            Assert.IsTrue(result is not null);
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
            var result = await _provider.GetImageAsync(repositoryId);

            // Assert
            Assert.IsTrue(result is null);
            _mockRepository.Verify(repo => repo.GetImageAsync(repositoryId), Times.Once);
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
            Assert.IsTrue(result is null);
            _mockRepository.Verify(repo => repo.GetImageAsync(repositoryId), Times.Never);
        }

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
            var result = await _provider.DeleteFromDatabaseAsync(testImageId);

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
            var result = await _provider.DeleteFromDatabaseAsync(testImageId);

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
            var result = await _provider.DeleteFromDatabaseAsync(testImageId);

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
            var result = await _provider.DeleteFromDatabaseAsync(repositoryId);

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
            var result = await _provider.DeleteFromDatabaseAsync(repositoryId);

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
            var result = await _provider.DeleteFromDatabaseAsync(repositoryId);

            // Assert
            Assert.IsFalse(result);
            _mockRepository.Verify(repo => repo.DeleteImageAsync(repositoryId), Times.Never);
        }

        /*  CONSIDER:
            Error Handling:
                Test how the provider handles exceptions thrown by the repository or the policy
            Policy Behavior:
                You might want to create specialized tests to verify that the retry policy is correctly applied in case of transient failures.
        */
    }
}
