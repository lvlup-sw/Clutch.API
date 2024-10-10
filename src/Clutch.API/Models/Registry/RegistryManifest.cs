using System.ComponentModel.DataAnnotations;

namespace Clutch.API.Models.Registry
{
    public record RegistryManifest
    {
        [Required]
        public int SchemaVersion { get; init; }

        [Required]
        [StringLength(255)]
        public required string MediaType { get; init; }

        [Required]
        public required ManifestConfig Config { get; init; }

        [Required]
        public required List<ManifestConfig> Layers { get; init; }

        public Dictionary<string, string>? Labels { get; init; }
    }
}
