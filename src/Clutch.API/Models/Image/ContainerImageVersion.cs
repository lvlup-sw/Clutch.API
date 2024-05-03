namespace Clutch.API.Models.Image
{
    public class ContainerImageVersion
    {
        public required string Repository { get; set; }

        public required string Tag { get; set; }

        public DateTime BuildDate { get; set; }

        public RegistryType RegistryType { get; set; }

        public StatusEnum Status { get; set; }        
    }
}
