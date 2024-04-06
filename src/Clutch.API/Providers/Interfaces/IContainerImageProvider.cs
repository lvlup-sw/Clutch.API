using CacheProvider.Providers.Interfaces;
using Clutch.API.Models.Image;

namespace Clutch.API.Providers.Interfaces
{
    public interface IContainerImageProvider : IRealProvider<ContainerImageModel>
    {
        Task<ContainerImageModel?> GetImageByIdAsync(int imageId);
        Task<ContainerImageModel?> GetImageByReferenceAsync(string imageReference);
        Task<IEnumerable<ContainerImageModel>?> GetLatestImagesAsync();
        Task<bool> SetImageAsync(ContainerImageModel image);
        Task<bool> DeleteFromDatabaseAsync(int imageId);
        Task<bool> DeleteFromDatabaseAsync(string imageReference);
    }
}
