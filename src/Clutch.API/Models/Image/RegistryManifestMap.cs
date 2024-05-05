using AutoMapper;

namespace Clutch.API.Models.Image
{
    // This is an AutoMapper profile to map between the ContainerImageModel and the ContainerImage.
    // AutoMapper can automatically map nested objects and collections between different data structures.
    // Thus it saves us from having to write manual mapping functions.
    public class RegistryManifestMap : Profile
    {
        public RegistryManifestMap()
        {
            CreateMap<RegistryManifestModel, RegistryManifest>();
            CreateMap<RegistryManifest, RegistryManifestModel>();
        }
    }
}