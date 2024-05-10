﻿using Microsoft.EntityFrameworkCore;
using Clutch.API.Database.Context;
using Clutch.API.Models.Image;
using Clutch.API.Repositories.Interfaces;

// Repository Responsibilities:
// - Directly manipulate the database.
// - Specifically, the container_images table.
namespace Clutch.API.Repositories.Image
{
    public class ContainerImageRepository(ContainerImageContext context, ILogger logger) : IContainerImageRepository
    {
        private readonly ContainerImageContext _context = context;
        private readonly ILogger _logger = logger;

        public async Task<ContainerImageModel> GetImageByIdAsync(int imageId)
        {
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

        public async Task<ContainerImageModel> GetImageByReferenceAsync(string repositoryId)
        {
            try
            {
                _logger.LogDebug("Getting image from the DB.");
                return await _context.ContainerImages
                    .Where(img => img.RepositoryId.Contains(repositoryId))
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
            try
            {
                // Auto-incrementing keys are always 0
                if (newImage is null || newImage.ImageID != 0) return false;

                _logger.LogDebug("Adding new image to the DB.");
                _context.ContainerImages.Add(newImage);
                int entries = await _context.SaveChangesAsync();

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