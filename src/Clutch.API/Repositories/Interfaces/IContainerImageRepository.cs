using Clutch.API.Models.Image;

namespace Clutch.API.Repositories.Interfaces
{
    public interface IContainerImageRepository
    {
        Task<ContainerImageModel> GetImageAsync(int imageId);
        Task<ContainerImageModel> GetImageAsync(string repository);
        Task<IEnumerable<ContainerImageModel>> GetLatestImagesAsync();
        Task<bool> SetImageAsync(ContainerImageModel image);
        Task<bool> DeleteImageAsync(int imageId);
        Task<bool> DeleteImageAsync(string repository);
    }
}