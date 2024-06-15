using Clutch.API.Models.Enums;
using Clutch.API.Models.Image;
using Clutch.API.Models.Registry;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Clutch.API.Utilities;

namespace Clutch.API.Tests
{
    public class TestUtils
    {
        private static IMapper? _mapper;

        public static void InitializeMapper(IMapper mapper) => _mapper = mapper;

        public static ContainerImageModel GetContainerImage(int testImageId)
        {
            return new()
            {
                ImageID = testImageId,
                RepositoryId = "test/image:latest",
                Repository = "test/image",
                Tag = "latest",
                BuildDate = DateTime.MaxValue,
                RegistryType = RegistryType.Docker,
                Status = StatusEnum.Available,
                Version = "1.0.0.0"
            };
        }

        public static ContainerImageModel GetContainerImageByReference(string repositoryId)
        {
            return new()
            {
                ImageID = 1,
                RepositoryId = repositoryId,
                Repository = repositoryId[..repositoryId.IndexOf(':')],
                Tag = "latest",
                BuildDate = DateTime.MaxValue,
                RegistryType = RegistryType.Docker,
                Status = StatusEnum.Available,
                Version = "1.0.0.0"
            };
        }

        public static ContainerImageModel GetContainerImageByRequest(string repository, string tag, RegistryType registry)
        {
            return new()
            {
                ImageID = 0,
                RepositoryId = $"{repository}:{tag}",
                Repository = repository,
                Tag = tag,
                BuildDate = DateTime.MaxValue,
                RegistryType = registry,
                Status = StatusEnum.Available,
                Version = "1.0.0.0"
            };
        }

        public static RegistryManifest GetRegistryManifest()
        {
            return new()
            {
                SchemaVersion = 2,
                MediaType = "application/vnd.docker.distribution.manifest.v2+json",
                Config = new()
                {
                    MediaType = "application/vnd.docker.container.image.v1+json",
                    Size = 3617,
                    Digest = "sha256:2f7f1f487714dc128bda7991936fd0dc16bcfd642f6bd4eb3a5128279a951999"
                },
                Layers =
                [
                    new ManifestConfig
                    { 
                        MediaType = "application/vnd.docker.image.rootfs.diff.tar.gzip",
                        Size = 29124181,
                        Digest = "sha256:8a1e25ce7c4f75e372e9884f8f7b1bedcfe4a7a7d452eb4b0a1c7477c9a90345"
                    },
                    new ManifestConfig
                    {
                        MediaType = "application/vnd.docker.image.rootfs.diff.tar.gzip",
                        Size = 18715444,
                        Digest = "sha256:a903092de2dbe1032be255f6aa13a67ae083a3f265fd27233b508751272f445a"
                    },
                    new ManifestConfig
                    {
                        MediaType = "application/vnd.docker.image.rootfs.diff.tar.gzip",
                        Size = 32227849,
                        Digest = "sha256:03ed6a6adf56f6f715105f0deee0751926934d40efc896e37dedb449be4fff73"
                    }
                ]
            };
        }

        public static ContainerImageResponseData GetContainerImageResponseData(string repository, string tag, RegistryType registry)
        {
            return new(true, GetContainerImageByRequest(repository, tag, registry), GetRegistryManifestModel());
        }

        public static ContainerImageResponse GetContainerImageResponse(string repository, string tag, RegistryType registry)
        {
            var mapper = GetContainerImageMapper();
            var image = mapper.Map<ContainerImage>(GetContainerImageByRequest(repository, tag, registry));
            return new(true, image, GetRegistryManifest());
        }

        public static RegistryManifestModel GetRegistryManifestModel() 
        {
            var mapper = GetRegistryMapper();
            return mapper.Map<RegistryManifestModel>(GetRegistryManifest());
        }

        private static IMapper GetRegistryMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<RegistryManifestModel, RegistryManifest>();
                cfg.CreateMap<RegistryManifest, RegistryManifestModel>();
            });

            return config.CreateMapper();
        }

        private static IMapper GetContainerImageMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ContainerImageModel, ContainerImage>();
                cfg.CreateMap<ContainerImage, ContainerImageModel>();
            });

            return config.CreateMapper();
        }

        public static bool ResponseDataAreEqual(ContainerImageResponseData expectedResponse, ContainerImageResponseData result)
        {
            return
                result.Success == expectedResponse.Success &&
                ImagesAreEqual(result.ContainerImageModel, expectedResponse.ContainerImageModel) &&
                ManifestsAreEqual(result.RegistryManifestModel, expectedResponse.RegistryManifestModel);
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

        public static bool ManifestsAreEqual(RegistryManifestModel expectedManifest, RegistryManifestModel result)
        {
            return
                result.SchemaVersion == expectedManifest.SchemaVersion &&
                result.MediaType.Equals(expectedManifest.MediaType) &&
                result.Config.Equals(expectedManifest.Config) &&
                result.Layers.SequenceEqual(expectedManifest.Layers);
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

        public static ContainerImageRequest GetInvalidContainerImageRequest()
        {
            return new()
            {
                Repository = "dog",
                Tag = "latest",
                RegistryType= RegistryType.Invalid
            };
        }

        public static string ConstructCacheKey(ContainerImageRequest request, string version)
        {
            var hash = CacheKeyGenerator.GenerateCacheKey(request, version);
            return $"{version}:{request.Repository}:{request.Tag}:{hash}";
        }
    }
}
