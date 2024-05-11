using Clutch.API.Models.Enums;
using Clutch.API.Models.Image;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace Clutch.API.Tests
{
    public class TestUtils
    {
        public static ContainerImageModel GetContainerImage(int testImageId)
        {
            return new ContainerImageModel
            {
                ImageID = testImageId,
                RepositoryId = "test/image:latest",
                Repository = "test/image",
                Tag = "latest",
                BuildDate = DateTime.MaxValue,
                RegistryType = RegistryType.Docker,
                Status = StatusEnum.Available,
                Version = "1.0.0"
            };
        }

        public static ContainerImageModel GetContainerImageByReference(string repositoryId)
        {
            return new ContainerImageModel
            {
                ImageID = 1,
                RepositoryId = repositoryId,
                Repository = repositoryId[..repositoryId.IndexOf(':')],
                Tag = "latest",
                BuildDate = DateTime.MaxValue,
                RegistryType = RegistryType.Docker,
                Status = StatusEnum.Available,
                Version = "1.0.0"
            };
        }

        public static ContainerImageModel GetContainerImageByRequest(string repository, string tag, RegistryType registry)
        {
            return new ContainerImageModel
            {
                ImageID = 0,
                RepositoryId = $"{repository}:{tag}",
                Repository = repository,
                Tag = tag,
                BuildDate = DateTime.MaxValue,
                RegistryType = registry,
                Status = StatusEnum.Available,
                Version = "1.0.0"
            };
        }

        public static bool ImagesAreEqual(ContainerImageModel expectedImage, ContainerImageModel result)
        {
            return
                result.ImageID == expectedImage.ImageID &&
                result.RepositoryId == expectedImage.RepositoryId &&
                result.Repository == expectedImage.Repository &&
                result.Tag == expectedImage.Tag &&
                result.BuildDate == expectedImage.BuildDate &&
                result.RegistryType == expectedImage.RegistryType &&
                result.Status == expectedImage.Status &&
                result.Version == expectedImage.Version;
        }

        public static ContainerImageRequest GetContainerImageRequest(string repository, string tag, RegistryType registry)
        {
            return new()
            {
                Repository = repository,
                Tag = tag,
                RegistryType= registry
            };
        }

        public static string ConstructCacheKey(ContainerImageRequest request, string version)
        {
            byte[] hash = SHA256.HashData(
                Encoding.UTF8.GetBytes(
                    JsonConvert.SerializeObject(request)
                )
            );

            return $"{version}:{request.Repository}:{request.Tag}:{string.Join("", hash.Select(b => b.ToString("x2"))).ToLower()}";
        }
    }
}
