using Clutch.API.Database.Context;
using Clutch.API.Models.Enums;
using Clutch.API.Models.Image;
using Clutch.API.Repositories.Image;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.Data.Sqlite;

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
            var result = await _repository.GetImageByIdAsync(testImageId);

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
            var result = await _repository.GetImageByIdAsync(nonExistentImageId);

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
            var result = await _repository.GetImageByReferenceAsync(repositoryId);

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
            var result = await _repository.GetImageByReferenceAsync(repositoryId);

            // Assert
            Assert.IsFalse(result.HasValue);
        }

        /*  TODO:
            1. SetImageAsync
                Success: New image with ID 0 properly inserted.
                Null Input: Attempt to insert a null image.
                Existing ID: Attempt to insert an image with a non-zero ID.

            2. DeleteImageAsync (both overloads)
                Success: Image found and deleted.
                Not Found: Image ID/Repository ID doesn't exist.
        */

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