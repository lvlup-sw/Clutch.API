using Clutch.API.Utilities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Clutch.API.Models.Image
{
    // This is a DTO (Data Transfer Object) for the ContainerImageModel
    // We use this to protect against overposting and to provide a more
    // limited view of only the necessary data to the client.
    public class ContainerImage
    {
        [Key]
        [Required]
        [StringLength(255)]
        public required string ImageReference { get; set; }

        [Required]
        public int GameVersion { get; set; } // BuildID number

        [Url]
        [Required]
        [StringLength(100)]
        public required string RegistryURL { get; set; }

        // We will need to add some sort of validation here via custom attribute
        // It may be a good idea to create a model class with a foreign key relationship instead
        public List<Plugin>? Plugins { get; set; }
    }
}
