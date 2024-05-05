using System.ComponentModel.DataAnnotations;

namespace Clutch.API.Models.Registry
{
    public class RegistryManifestModel
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

        public bool HasValue => !IsNullOrEmpty;

        private bool IsNullOrEmpty
        {
            get
            {
                if (string.IsNullOrEmpty(MediaType)) return true;
                if (Config.Count == 0) return true;
                if (Layers.Count == 0) return true;
                return false;
            }
        }

        public static RegistryManifestModel Null { get; } = new()
        {
            MediaType = string.Empty,
            Config = new() { Digest = string.Empty, MediaType = string.Empty, Size = 0 },
            Layers = [],
        };
    }
}
