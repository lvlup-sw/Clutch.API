using System.ComponentModel.DataAnnotations;

// This is an immutable read-only DTO for our
// database model class. This object is what
// we return to the client.
namespace Clutch.API.Models.Image
{
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
