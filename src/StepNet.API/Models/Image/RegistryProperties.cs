using static System.Net.Mime.MediaTypeNames;

namespace StepNet.API.Models.Image
{
    public class RegistryProperties
    {
        // Add GHCR properties
        public bool FillerProperty { get; set; }

        public bool HasValue => !IsNullOrEmpty;

        private bool IsNullOrEmpty
        {
            get
            {
                if (FillerProperty is false) return true;
                return false;
            }
        }

        public static RegistryProperties Null { get; } = new()
        {
            FillerProperty = false
        };
    }
}
