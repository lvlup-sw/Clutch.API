using AutoMapper;

namespace Clutch.API.Models.Image
{
    // This is an AutoMapper profile to map between the ContainerImageModel and the ContainerImage.
    // AutoMapper can automatically map nested objects and collections between different data structures.
    // Thus it saves us from having to write manual mapping functions.
    public class ContainerImageMap : Profile
    {
        public ContainerImageMap()
        {
            CreateMap<ContainerImageModel, ContainerImage>();
            CreateMap<ContainerImage, ContainerImageModel>()
                .ForMember(dest => dest.HasValue, opt => opt.Ignore());
        }
    }
}