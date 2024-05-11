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
            var expectedImage = TestUtils.GetContainerImage(testImageId);

            // Seed database
            _context.ContainerImages.Add(TestUtils.GetContainerImage(testImageId));
            _context.SaveChanges();

            // Act
            var result = await _repository.GetImageAsync(testImageId);

            // Assert
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
            // Seed database
            _context.ContainerImages.Add(TestUtils.GetContainerImage(5));
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
            var expectedImage = TestUtils.GetContainerImageByReference(repositoryId);

            // Seed database
            _context.ContainerImages.Add(TestUtils.GetContainerImageByReference(repositoryId));
            _context.SaveChanges();

            // Act
            var result = await _repository.GetImageAsync(repositoryId);

            // Assert
            Assert.IsTrue(TestUtils.ImagesAreEqual(expectedImage, result));
        }

        [DataTestMethod]
        [DataRow("test/image:latest")]
        [DataRow("joedward32/cs2:latest")]
        [DataRow(default)]
        [TestMethod]
        public async Task GetImageByReferenceAsync_NotFound(string repositoryId)
        {
            // Arrange
            // Seed database
            _context.ContainerImages.Add(TestUtils.GetContainerImageByReference("tensorflow/tensorflow:latest"));
            _context.SaveChanges();

            // Act
            var result = await _repository.GetImageAsync(repositoryId);

            // Assert
            Assert.IsFalse(result.HasValue);
        }

        // DataRow doesn't allow for dynamically created data structures
        [TestMethod]
        public async Task SetImageAsync_Success1()
        {
            // Arrange
            var request = TestUtils.GetContainerImageByRequest("test/image", "latest", RegistryType.Docker);

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
            var request = TestUtils.GetContainerImageByRequest("joedwards32/cs2", "latest", RegistryType.Docker);

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
            var request = TestUtils.GetContainerImageByRequest("lvlup-sw/clutchapi", "dev", RegistryType.Local);

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
            var request = TestUtils.GetContainerImageByRequest("", "", RegistryType.Invalid);

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

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(6)]
        [DataRow(99)]
        [TestMethod]
        public async Task DeleteImageByIdAsync_Success(int testImageId)
        {
            // Arrange
            // Seed database
            _context.ContainerImages.Add(TestUtils.GetContainerImage(testImageId));
            _context.SaveChanges();

            // Act
            var result = await _repository.DeleteImageAsync(testImageId);

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
            _context.ContainerImages.Add(TestUtils.GetContainerImageByReference(repositoryId));
            _context.SaveChanges();

            // Act
            var result = await _repository.DeleteImageAsync(repositoryId);

            // Assert
            Assert.IsTrue(result);
            Assert.IsFalse(_context.ContainerImages.Any());
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(99)]
        [DataRow(0)]
        [TestMethod]
        public async Task DeleteImageByIdAsync_NotFound(int testImageId)
        {
            // Arrange
            // Seed database
            _context.ContainerImages.Add(TestUtils.GetContainerImage(5));
            _context.SaveChanges();

            // Act
            var result = await _repository.DeleteImageAsync(testImageId);

            // Assert
            Assert.IsFalse(result);
            Assert.IsTrue(_context.ContainerImages.Contains(TestUtils.GetContainerImage(5)));
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
            _context.ContainerImages.Add(TestUtils.GetContainerImageByReference("tensorflow/tensorflow:latest"));
            _context.SaveChanges();

            // Act
            var result = await _repository.DeleteImageAsync(repositoryId);

            // Assert
            Assert.IsFalse(result);
            Assert.IsTrue(_context.ContainerImages.Contains(TestUtils.GetContainerImageByReference("tensorflow/tensorflow:latest")));
        }
    }
}