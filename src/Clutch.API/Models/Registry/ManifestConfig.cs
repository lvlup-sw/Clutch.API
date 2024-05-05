namespace Clutch.API.Models.Registry
{
    public class ManifestConfig : Dictionary<string, object>
    {
        public required string Digest { get; set; }
        public required string MediaType { get; set; }
        public required int Size { get; set; }
    }
}
