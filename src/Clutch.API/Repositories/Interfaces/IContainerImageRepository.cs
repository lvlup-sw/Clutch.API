using Clutch.API.Models.Image;

namespace Clutch.API.Repositories.Interfaces
{
    public interface IContainerImageRepository
    {
        Task<ContainerImageModel> GetImageByIdAsync(int imageId);
        Task<ContainerImageModel> GetImageByReferenceAsync(string Repository);
        Task<IEnumerable<ContainerImageModel>> GetLatestImagesAsync();
        Task<bool> SetImageAsync(ContainerImageModel image);
        Task<bool> DeleteImageAsync(int imageId);
        Task<bool> DeleteImageAsync(string Repository);
    }
}