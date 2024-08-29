using AutoMapper;

namespace Clutch.API.Models.Registry
{
    // This is an AutoMapper profile to map between the RegistryManifestModel and the RegistryManifest.
    // AutoMapper can automatically map nested objects and collections between different data structures.
    // Thus it saves us from having to write manual mapping functions.
    public class RegistryManifestMap : Profile
    {
        public RegistryManifestMap()
        {
            CreateMap<RegistryManifestModel, RegistryManifest>();
            CreateMap<RegistryManifest, RegistryManifestModel>()
                .ForMember(dest => dest.HasValue, opt => opt.Ignore());
        }
    }
}