using System.ComponentModel.DataAnnotations;

namespace Clutch.API.Models.Image
{
    public class ContainerImage
    {
        [Required]
        [StringLength(255)]
        public required string Repository { get; set; }

        [Required]
        [StringLength(128)]
        public required string Tag { get; set; }

        [Required]
        public DateTime BuildDate { get; set; }

        [Required]
        public RegistryType RegistryType { get; set; }

        [Required]
        public StatusEnum Status { get; set; }        
    }
}
