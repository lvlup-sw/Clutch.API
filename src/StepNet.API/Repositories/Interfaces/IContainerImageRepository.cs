using StepNet.API.Models.Image;

namespace StepNet.API.Repositories.Interfaces
{
    public interface IContainerImageRepository
    {
        Task<ContainerImageModel> GetImageByIdAsync(int imageId);
        Task<ContainerImageModel> GetImageByReferenceAsync(string imageReference);
        Task<IEnumerable<ContainerImageModel>> GetLatestImagesAsync();
        Task<bool> SetImageAsync(ContainerImageModel image);
        Task<bool> DeleteImageAsync(int imageId);
        Task<bool> DeleteImageAsync(string imageReference);
    }
}