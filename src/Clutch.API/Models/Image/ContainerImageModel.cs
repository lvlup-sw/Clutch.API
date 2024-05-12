using Clutch.API.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

// This is our actual Database model, and what we use to perform our operations.
// We do not directly use ContainerImageVersion to limit visibility into our application.
// This protects us from over-posting attacks.
// We have two indexes: ImageId and RepositoryId.
// Ideally we use ImageId where possible.
namespace Clutch.API.Models.Image
{
    [Index(nameof(RepositoryId), IsUnique = true)]
    public class ContainerImageModel
    {
        [Key]
        [Required]
        public int ImageID { get; set; } = 0;

        [Required]
        [StringLength(384)]
        public required string RepositoryId { get; set; }

        [Required]
        [StringLength(255)]
        public required string Repository { get; set; }

        [Required]
        [StringLength(128)] // Dockerhub limit is 128 characters
        public required string Tag { get; set; }

        [Required]
        public DateTime BuildDate { get; set; }

        [Required]
        public RegistryType RegistryType { get; set; }

        [Required]
        public StatusEnum Status { get; set; }

        [Required]
        public required string Version { get; set; }

        public bool HasValue => !IsNullOrEmpty;

        private bool IsNullOrEmpty
        {
            get
            {
                // Required items
                if (ImageID == -1) return true;
                if (string.IsNullOrEmpty(RepositoryId)) return true;
                if (string.IsNullOrEmpty(Repository)) return true;
                if (string.IsNullOrEmpty(Tag)) return true;
                if (BuildDate == DateTime.MinValue) return true;
                if (RegistryType == RegistryType.Invalid) return true;
                if (Status == StatusEnum.Invalid) return true;
                if (string.IsNullOrEmpty(Version)) return true;
                return false;
            }
        }

        public static ContainerImageModel Null { get; } = new()
        {
            ImageID = -1,
            RepositoryId = string.Empty,
            Repository = string.Empty,
            Tag = string.Empty,
            BuildDate = DateTime.MinValue,
            RegistryType = RegistryType.Invalid,
            Status = StatusEnum.Invalid,
            Version = string.Empty
        };
    }
}