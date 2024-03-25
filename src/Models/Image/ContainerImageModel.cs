using StepNet.API.Utilities;
using System.ComponentModel.DataAnnotations;

namespace StepNet.API.Models.Image
{
    public class ContainerImageModel
    {
        [Key]
        public int ImageID { get; set; }

        [Required]
        [StringLength(100)]
        public string ImageName { get; set; }

        [Required]
        [StringLength(20)]
        public string GameVersion { get; set; }

        [Required]
        public DateTime BuildDate { get; set; }

        [Required]
        [StringLength(255)]
        public string ManifestDigest { get; set; }

        [Required]
        [StringLength(100)]
        public string RegistryURL { get; set; }

        [StringLength(40)]
        public string? CommitHash { get; set; }

        public int? Layers { get; set; } // Nullable for potential flexibility

        public long? Size { get; set; } // Nullable for the same reason

        // This is stored in the DB as a JSON string with metadata associated with each plugin
        // We create a class to represent the JSON object to make it easier to work with
        public List<Plugin>? Plugins { get; set; }

        public string? ServerConfig { get; set; }

        public string? Notes { get; set; }
    }
}