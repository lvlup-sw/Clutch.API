namespace Clutch.API.Models.Image
{
    public class ManifestConfig : Dictionary<string, object>
    {
        public required string Digest { get; set; }
        public required string MediaType { get; set; }
        public required int Size { get; set; }
    }
}
