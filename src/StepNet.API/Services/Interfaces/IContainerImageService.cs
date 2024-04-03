using StepNet.API.Models.Image;

namespace StepNet.API.Services.Interfaces
{
    public interface IContainerImageService
    {
        Task<ContainerImageResponseData> GetImageAsync(string imageReference);
        Task<bool> SetImageAsync(ContainerImageModel containerImageModel);
        Task<bool> DeleteImageAsync(string imageReference);
        Task<IEnumerable<ContainerImageModel>?> GetLatestImagesAsync();
    }
}
