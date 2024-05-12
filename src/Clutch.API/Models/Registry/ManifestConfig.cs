namespace Clutch.API.Models.Registry
{
    public class ManifestConfig
    {
        public required string Digest { get; set; }
        public required string MediaType { get; set; }
        public required int Size { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is not ManifestConfig other) return false;

            return
                Digest.Equals(other.Digest) &&
                MediaType.Equals(other.MediaType) &&
                Size.Equals(other.Size);
        }

        public override int GetHashCode() => base.GetHashCode();
    }
}
