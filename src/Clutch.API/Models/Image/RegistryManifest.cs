using static System.Net.Mime.MediaTypeNames;

namespace Clutch.API.Models.Image
{
    public class RegistryManifest
    {
        public int schemaVersion { get; set; }
        public required string mediaType { get; set; }
        public required ManifestConfig config { get; set; }
        public required List<ManifestConfig> layers { get; set; }
        public Dictionary<string, string>? labels { get; set; }

        public bool HasValue => !IsNullOrEmpty;

        private bool IsNullOrEmpty
        {
            get
            {
                if (mediaType.Equals(Application.Json)) return true;
                if (config.Count == 0) return true;
                if (layers.Count == 0) return true;
                return false;
            }
        }

        public static RegistryManifest Null { get; } = new()
        {
            mediaType = Application.Json,
            config = new(){ digest = string.Empty, mediaType = string.Empty, size = 0 },
            layers = [],
        };
    }

    public class ManifestConfig : Dictionary<string, object>
    {
        public required string digest { get; set; }
        public required string mediaType { get; set; }
        public required int size { get; set; }
    }
}
