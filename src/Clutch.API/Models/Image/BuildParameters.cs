namespace Clutch.API.Models.Image
{
    // Skeleton class for the build parameters of a container image
    public class BuildParameters
    {
        public string? Repository { get; set; }
        public DateTime? BuildDate { get; set; }
        public string? Tag { get; set; }
        // Build Args
        public string Branch { get; set; } = "main";
        public string DockerfilePath { get; set; } = "Dockerfile";
        public Dictionary<string, string>? BuildArgs { get; set; }
    }
}
