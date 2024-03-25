using Microsoft.EntityFrameworkCore;
using StepNet.API.Models.Image;
using StepNet.API.Repositories.Interfaces;

namespace StepNet.API.Repositories.Image
{
    public class ContainerImageRepository : IContainerImageRepository
    {
        private readonly ContainerImageContext _context;

        public ContainerImageRepository(ContainerImageContext context)
        {
            _context = context;
        }

        public async Task<ContainerImageModel> GetImageByIdAsync(int imageId)
        {
            throw new NotImplementedException();
        }

        public async Task SetImageAsync(ContainerImageModel newImage)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteImageAsync(int imageId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ContainerImageModel>> GetAllImagesAsync()
        {
            return await _context.ContainerImages.ToListAsync();
        }
    }
}
