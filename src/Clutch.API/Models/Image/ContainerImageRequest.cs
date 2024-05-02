using Clutch.API.Utilities;
using System.ComponentModel.DataAnnotations;

namespace Clutch.API.Models.Image
{
    public class ContainerImage
    {
        [Key]
        [Required]
        [StringLength(255)]
        public required string Repository { get; set; }

        [Required]
        public required string Tag { get; set; } // BuildID number

        [Required]
        public required RegistryType RegistryType { get; set; }

        public string? Digest 
    }
}
