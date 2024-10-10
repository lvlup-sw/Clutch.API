using System.ComponentModel.DataAnnotations;

namespace Clutch.API.Models.Image
{
    // This is an immutable DTO and is what we return to the client
    public record ContainerImage
    {
        [Required]
        [StringLength(255)]
        public required string Repository { get; init; }

        [Required]
        [StringLength(128)]
        public required string Tag { get; init; }

        [Required]
        public DateTime BuildDate { get; init; }

        [Required]
        public RegistryType RegistryType { get; init; }

        [Required]
        public StatusEnum Status { get; init; }
    }
}
