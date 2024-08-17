using Microsoft.Azure.Amqp.Framing;
using Microsoft.EntityFrameworkCore;

// Repository Responsibilities:
// - Directly manipulate the database.
// - Specifically, the container_images table.
namespace Clutch.API.Repositories.Image
{
    public class ContainerImageRepository(ContainerImageContext context, ILogger logger) : IContainerImageRepository
    {
        private readonly ContainerImageContext _context = context;
        private readonly ILogger _logger = logger;

        public async Task<ContainerImageModel> GetImageAsync(int imageId)
        {
            if (imageId < 1) return ContainerImageModel.Null;

            try
            {
                _logger.LogDebug("Getting image from the DB.");
                return await _context.ContainerImages
                    .Where(img => img.ImageID == imageId)
                    .FirstOrDefaultAsync() ?? ContainerImageModel.Null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetBaseException(), "Error getting the image from the DB.");
                return ContainerImageModel.Null;
            }
        }

        public async Task<ContainerImageModel> GetImageAsync(string repositoryId)
        {
            if (string.IsNullOrEmpty(repositoryId)) return ContainerImageModel.Null;

            try
            {
                _logger.LogDebug("Getting image from the DB.");
                return await _context.ContainerImages
                    .Where(img => img.RepositoryId.Equals(repositoryId))
                    .FirstOrDefaultAsync() ?? ContainerImageModel.Null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetBaseException(), "Error getting the image from the DB.");
                return ContainerImageModel.Null;
            }
        }

        public async Task<IEnumerable<ContainerImageModel>> GetLatestImagesAsync()
        {
            try
            {
                _logger.LogDebug("Getting all images from the DB.");
                return await _context.ContainerImages.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetBaseException(), "Error getting all images from the DB.");
                return await Task.FromResult(Enumerable.Empty<ContainerImageModel>());
            }
        }

        public async Task<bool> SetImageAsync(ContainerImageModel newImage)
        {
            // Auto-incrementing keys are always 0
            if (!newImage.HasValue || newImage.ImageID != 0) return false;

            try
            {
                _logger.LogDebug("Setting image in the DB.");

                // Update existing image if it exists
                // We specify each property explicitly
                // because we can't modify ImageId
                int entries = await _context.ContainerImages
                    .Where(img => img.RepositoryId == newImage.RepositoryId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(p => p.RepositoryId, newImage.RepositoryId)
                        .SetProperty(p => p.Repository, newImage.Repository)
                        .SetProperty(p => p.Tag, newImage.Tag)
                        .SetProperty(p => p.BuildDate, newImage.BuildDate)
                        .SetProperty(p => p.RegistryType, newImage.RegistryType)
                        .SetProperty(p => p.Status, newImage.Status)
                        .SetProperty(p => p.Version, newImage.Version)
                    );

                // Ideally we could do something like this
                // but EF Core doesn't allow Indexes to be
                // modified whatsoever, even temporarily
                //_context.Entry(existingImage).CurrentValues.SetValues(newImage);

                // If entries = 0, it means the image didn't exist
                if (entries == 0)
                {
                    _context.ContainerImages.Add(newImage);
                    entries = await _context.SaveChangesAsync();
                }

                return entries > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetBaseException(), "Error setting the image in the DB.");
                return false;
            }
        }

        public async Task<bool> DeleteImageAsync(int imageId)
        {
            if (imageId < 1) return false;

            try
            {
                int entries = 0;
                var imageToDelete = await _context.ContainerImages.FindAsync(imageId);
                if (imageToDelete is not null)
                {
                    _logger.LogDebug("Deleting image from the DB.");
                    _context.ContainerImages.Remove(imageToDelete);
                    entries = await _context.SaveChangesAsync();
                }

                return entries > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetBaseException(), "Error deleting the image from the DB.");
                return false;
            }
        }

        public async Task<bool> DeleteImageAsync(string repositoryId)
        {
            if (string.IsNullOrEmpty(repositoryId)) return false;

            try
            {
                int entries = 0;
                var imageToDelete = await _context.ContainerImages
                    .FirstOrDefaultAsync(img => img.RepositoryId == repositoryId);

                if (imageToDelete is not null)
                {
                    _logger.LogDebug("Deleting image from the DB.");
                    _context.ContainerImages.Remove(imageToDelete);
                    entries = await _context.SaveChangesAsync();
                }

                return entries > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.GetBaseException(), "Error deleting the image from the DB.");
                return false;
            }
        }
    }
}
