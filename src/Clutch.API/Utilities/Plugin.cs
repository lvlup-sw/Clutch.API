namespace Clutch.API.Utilities
{
    public class Plugin
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required string Version { get; set; }
        public string? URL { get; set; }
    }
}