using System.ComponentModel.DataAnnotations;

namespace Clutch.API.Models.Image
{
    // Skeleton class for the build parameters of a container image
    public class BuildParameters
    {
        public required string Repository { get; set; }
        public int GameVersion { get; set; } // BuildID number
        public DateTime BuildDate { get; set; }
        public required string RegistryURL { get; set; }
        // Build Args
        public string Branch { get; set; } = "main";
        public string DockerfilePath { get; set; } = "Dockerfile";
        public Dictionary<string, string>? BuildArgs { get; set; }
    }
}
