namespace Clutch.API.Models.Image
{
    public class RegistryManifest
    {
        public int SchemaVersion { get; set; }
        public required string MediaType { get; set; }
        public required ManifestConfig Config { get; set; }
        public required List<ManifestConfig> Layers { get; set; }
        public Dictionary<string, string>? Labels { get; set; }

        public bool HasValue => !IsNullOrEmpty;

        private bool IsNullOrEmpty
        {
            get
            {
                if (string.IsNullOrEmpty(MediaType)) return true;
                if (Config.Count == 0) return true;
                if (Layers.Count == 0) return true;
                return false;
            }
        }

        public static RegistryManifest Null { get; } = new()
        {
            MediaType = string.Empty,
            Config = new(){ Digest = string.Empty, MediaType = string.Empty, Size = 0 },
            Layers = [],
        };
    }

    public class ManifestConfig : Dictionary<string, object>
    {
        public required string Digest { get; set; }
        public required string MediaType { get; set; }
        public required int Size { get; set; }
    }
}
