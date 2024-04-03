using CacheProvider.Providers.Interfaces;
using StepNet.API.Models.Image;

namespace StepNet.API.Providers.Interfaces
{
    public interface IContainerImageProvider : IRealProvider<ContainerImageModel>
    {
        Task<ContainerImageModel?> GetImageByIdAsync(int imageId);
        Task<ContainerImageModel?> GetImageByReferenceAsync(string imageReference);
        Task<IEnumerable<ContainerImageModel>?> GetLatestImagesAsync();
        Task<bool> SetImageAsync(ContainerImageModel image);
        Task<bool> DeleteFromDatabaseAsync(int imageId);
        Task<bool> DeleteFromDatabaseAsync(string imageReference);
        Task<bool> DeleteFromRegistryAsync(string imageReference);
        Task<ContainerImageBuildResult> TriggerBuildAsync(BuildParameters parameters);
        // Add interactions with other DB tables or services here
    }
}
