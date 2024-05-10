using Clutch.API.Database.Context;
using Clutch.API.Models.Enums;
using Clutch.API.Models.Image;
using Clutch.API.Repositories.Image;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Data.Sqlite;
using Moq;

namespace Clutch.API.Tests
{
    [TestClass]
    public class ContainerImageRepositoryTests
    {
        private Mock<ILogger> _mockLogger;
        private ContainerImageContext _context;
        private ContainerImageRepository _repository;

        [TestInitialize]
        public void Setup()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ContainerImageContext>()
                .UseSqlite(connection)
                .Options;

            _context = new ContainerImageContext(options);
            _mockLogger = new Mock<ILogger>();

            // Setup database
            _context.Database.EnsureCreated();
            _context.Database.Migrate();

            _repository = new ContainerImageRepository(_context, _mockLogger.Object);
        }

        [TestCleanup]
        public void Cleanup() => _context.Dispose();

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(5)]
        [DataRow(10)]
        [TestMethod]
        public async Task GetImageByIdAsync_Success(int testImageId)
        {
            // Arrange
            var expectedImage = GetContainerImage(testImageId);

            // Seed database
            _context.ContainerImages.Add(GetContainerImage(testImageId));
            _context.SaveChanges();

            // Act
            var result = await _repository.GetImageAsync(testImageId);

            // Assert
            Assert.IsTrue(ImagesAreEqual(expectedImage, result));
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(6)]
        [DataRow(99)]
        [TestMethod]
        public async Task GetImageByIdAsync_NotFound(int nonExistentImageId)
        {
            // Arrange
            // Seed database
            _context.ContainerImages.Add(GetContainerImage(5));
            _context.SaveChanges();

            // Act
            var result = await _repository.GetImageAsync(nonExistentImageId);

            // Assert
            Assert.IsFalse(result.HasValue);
        }

        [DataTestMethod]
        [DataRow("test/image:latest")]
        [DataRow("joedward32/cs2:latest")]
        [DataRow("lvlup-sw/clutchapi:dev")]
        [TestMethod]
        public async Task GetImageByReferenceAsync_Success(string repositoryId)
        {
            // Arrange
            var expectedImage = GetContainerImageByReference(repositoryId);

            // Seed database
            _context.ContainerImages.Add(GetContainerImageByReference(repositoryId));
            _context.SaveChanges();

            // Act
            var result = await _repository.GetImageAsync(repositoryId);

            // Assert
            Assert.IsTrue(ImagesAreEqual(expectedImage, result));
        }

        [DataTestMethod]
        [DataRow("test/image:latest")]
        [DataRow("joedward32/cs2:latest")]
        [DataRow("lvlup-sw/clutchapi:dev")]
        [TestMethod]
        public async Task GetImageByReferenceAsync_NotFound(string repositoryId)
        {
            // Arrange
            // Seed database
            _context.ContainerImages.Add(GetContainerImageByReference("tensorflow/tensorflow:latest"));
            _context.SaveChanges();

            // Act
            var result = await _repository.GetImageAsync(repositoryId);

            // Assert
            Assert.IsFalse(result.HasValue);
        }

        // DataRow doesn't allow for dynamically created items (aka objects)
        [TestMethod]
        public async Task SetImageAsync_Success1()
        {
            // Arrange
            var request = GetContainerImageByRequest("test/image", "latest", RegistryType.Docker);

            // Act
            var result = await _repository.SetImageAsync(request);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(_context.ContainerImages.Any());
        }

        [TestMethod]
        public async Task SetImageAsync_Success2()
        {
            // Arrange
            var request = GetContainerImageByRequest("joedwards32/cs2", "latest", RegistryType.Docker);

            // Act
            var result = await _repository.SetImageAsync(request);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(_context.ContainerImages.Any());
        }

        [TestMethod]
        public async Task SetImageAsync_Success3()
        {
            // Arrange
            var request = GetContainerImageByRequest("lvlup-sw/clutchapi", "dev", RegistryType.Local);

            // Act
            var result = await _repository.SetImageAsync(request);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(_context.ContainerImages.Any());
        }

        [TestMethod]
        public async Task SetImageAsync_Failure1_WithInvalid()
        {
            // Arrange
            var request = GetContainerImageByRequest("", "", RegistryType.Invalid);

            // Act
            var result = await _repository.SetImageAsync(request);

            // Assert
            Assert.IsFalse(result);
            Assert.IsFalse(_context.ContainerImages.Any());
        }

        [TestMethod]
        public async Task SetImageAsync_Failure2_WithNull()
        {
            // Arrange
            var request = ContainerImageModel.Null;

            // Act
            var result = await _repository.SetImageAsync(request);

            // Assert
            Assert.IsFalse(result);
            Assert.IsFalse(_context.ContainerImages.Any());
        }

        [TestMethod]
        public async Task SetImageAsync_Failure3_WithNull()
        {
            // Arrange
            ContainerImageModel? request = null;

            // Act
            var result = await _repository.SetImageAsync(request);

            // Assert
            Assert.IsFalse(result);
            Assert.IsFalse(_context.ContainerImages.Any());
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(6)]
        [DataRow(99)]
        [TestMethod]
        public async Task DeleteImageByIdAsync_Success(int imageId)
        {
            // Arrange
            // Seed database
            _context.ContainerImages.Add(GetContainerImage(imageId));
            _context.SaveChanges();

            // Act
            var result = await _repository.DeleteImageAsync(imageId);

            // Assert
            Assert.IsTrue(result);
            Assert.IsFalse(_context.ContainerImages.Any());
        }

        [DataTestMethod]
        [DataRow("test/image:latest")]
        [DataRow("joedward32/cs2:latest")]
        [DataRow("lvlup-sw/clutchapi:dev")]
        [TestMethod]
        public async Task DeleteImageByReferenceAsync_Success(string repositoryId)
        {
            // Arrange
            // Seed database
            _context.ContainerImages.Add(GetContainerImageByReference(repositoryId));
            _context.SaveChanges();

            // Act
            var result = await _repository.DeleteImageAsync(repositoryId);

            // Assert
            Assert.IsTrue(result);
            Assert.IsFalse(_context.ContainerImages.Any());
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(6)]
        [DataRow(99)]
        [TestMethod]
        public async Task DeleteImageByIdAsync_NotFound(int imageId)
        {
            // Arrange
            // Seed database
            _context.ContainerImages.Add(GetContainerImage(5));
            _context.SaveChanges();

            // Act
            var result = await _repository.DeleteImageAsync(imageId);

            // Assert
            Assert.IsFalse(result);
            Assert.IsTrue(_context.ContainerImages.Contains(GetContainerImage(5)));
        }

        [DataTestMethod]
        [DataRow("test/image:latest")]
        [DataRow("joedward32/cs2:latest")]
        [DataRow("lvlup-sw/clutchapi:dev")]
        [TestMethod]
        public async Task DeleteImageByReferenceAsync_NotFound(string repositoryId)
        {
            // Arrange
            // Seed database
            _context.ContainerImages.Add(GetContainerImageByReference("tensorflow/tensorflow:latest"));
            _context.SaveChanges();

            // Act
            var result = await _repository.DeleteImageAsync(repositoryId);

            // Assert
            Assert.IsFalse(result);
            Assert.IsTrue(_context.ContainerImages.Contains(GetContainerImageByReference("tensorflow/tensorflow:latest")));
        }

        private static ContainerImageModel GetContainerImage(int testImageId)
        {
            return new ContainerImageModel
            {
                ImageID = testImageId,
                RepositoryId = "test/image:latest",
                Repository = "test/image",
                Tag = "latest",
                BuildDate = DateTime.MaxValue,
                RegistryType = RegistryType.Docker,
                Status = StatusEnum.Available,
                Version = "1.0.0"
            };
        }

        private static ContainerImageModel GetContainerImageByReference(string repositoryId)
        {
            return new ContainerImageModel
            {
                ImageID = 1,
                RepositoryId = repositoryId,
                Repository = repositoryId[..repositoryId.IndexOf(':')],
                Tag = "latest",
                BuildDate = DateTime.MaxValue,
                RegistryType = RegistryType.Docker,
                Status = StatusEnum.Available,
                Version = "1.0.0"
            };
        }

        private static ContainerImageModel GetContainerImageByRequest(string repository, string tag, RegistryType registry)
        {
            return new ContainerImageModel
            {
                ImageID = 0,
                RepositoryId = $"{repository}:{tag}",
                Repository = repository,
                Tag = tag,
                BuildDate = DateTime.MaxValue,
                RegistryType = registry,
                Status = StatusEnum.Available,
                Version = "1.0.0"
            };
        }        

        private static bool ImagesAreEqual(ContainerImageModel expectedImage, ContainerImageModel result)
        {
            return
                result.ImageID == expectedImage.ImageID &&
                result.RepositoryId == expectedImage.RepositoryId &&
                result.Repository == expectedImage.Repository &&
                result.Tag == expectedImage.Tag &&
                result.BuildDate == expectedImage.BuildDate &&
                result.RegistryType == expectedImage.RegistryType &&
                result.Status == expectedImage.Status &&
                result.Version == expectedImage.Version;
        }
    }
}