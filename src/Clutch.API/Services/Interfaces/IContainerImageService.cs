using Clutch.API.Models.Image;

namespace Clutch.API.Services.Interfaces
{
    public interface IContainerImageService
    {
        Task<ContainerImageResponseData> GetImageAsync(string imageReference);
        Task<bool> SetImageAsync(ContainerImageModel containerImageModel);
        Task<bool> DeleteImageAsync(string imageReference);
        
        // Revisit these methods
        Task<IEnumerable<ContainerImageModel>?> GetLatestImagesAsync();
    }
}
