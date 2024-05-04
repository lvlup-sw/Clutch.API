using Clutch.API.Models.Image;

namespace Clutch.API.Services.Interfaces
{
    public interface IContainerImageService
    {
        Task<ContainerImageResponseData> GetImageAsync(ContainerImageRequest request, string version);
        Task<bool> SetImageAsync(ContainerImageRequest request, string version);
        Task<bool> DeleteImageAsync(ContainerImageRequest request, string version);
        
        // Revisit this method
        Task<IEnumerable<ContainerImageModel>?> GetLatestImagesAsync();
    }
}
