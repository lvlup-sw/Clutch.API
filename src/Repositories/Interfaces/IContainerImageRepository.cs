using StepNet.API.Models.Image;

namespace StepNet.API.Repositories.Interfaces
{
    public interface IContainerImageRepository
    {
        Task<IEnumerable<ContainerImageModel>> GetAllImagesAsync();
        Task SetImageAsync(ContainerImageModel newImage);
        Task DeleteImageAsync(int imageId);
        Task<ContainerImageModel> GetImageByIdAsync(int imageId);
    }
}