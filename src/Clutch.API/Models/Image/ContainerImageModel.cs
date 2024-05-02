using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Microsoft.EntityFrameworkCore;
using Clutch.API.Utilities;
using System.ComponentModel.DataAnnotations;

namespace Clutch.API.Models.Image
{
    [Index(nameof(ImageReference), IsUnique = true)]
    public class ContainerImageModel
    {
        [Key]
        [Required]
        public int ImageID { get; set; } = 0;

        [Required]
        [StringLength(255)]
        public required string ImageReference { get; set; }

        [Required]
        public int GameVersion { get; set; } // BuildID number

        [Required]
        public DateTime BuildDate { get; set; }

        [Url]
        [Required]
        [StringLength(100)]
        public required string RegistryURL { get; set; }

        [Required]
        public StatusEnum Status { get; set; }

        [StringLength(40)]
        public string? CommitHash { get; set; }

        public bool HasValue => !IsNullOrEmpty;

        private bool IsNullOrEmpty
        {
            get
            {
                // Indexed items
                if (ImageID == -1) return true;
                if (string.IsNullOrEmpty(ImageReference)) return true;
                return false;
            }
        }

        public static ContainerImageModel Null { get; } = new()
        {
            ImageID = -1,
            ImageReference = string.Empty,
            GameVersion = 0,
            BuildDate = DateTime.Now,
            RegistryURL = string.Empty,
            Status = StatusEnum.Unavailable,
            CommitHash = string.Empty,
        };
    }
}