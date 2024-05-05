using System.ComponentModel.DataAnnotations;

namespace Clutch.API.Models.Registry
{
    public class RegistryManifest
    {
        [Required]
        public int SchemaVersion { get; set; }

        [Required]
        [StringLength(255)]
        public required string MediaType { get; set; }

        [Required]
        public required ManifestConfig Config { get; set; }

        [Required]
        public required List<ManifestConfig> Layers { get; set; }

        public Dictionary<string, string>? Labels { get; set; }
    }
}
