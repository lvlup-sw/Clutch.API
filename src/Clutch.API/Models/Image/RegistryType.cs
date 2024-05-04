namespace Clutch.API.Models.Image
{
    public enum RegistryType
    {
        Invalid = -1,
        Local = 0,  // GHCR
        Docker = 1,
        Google = 2,
        AWS = 3,
        Azure = 4,
        Harbor = 5,
        Artifactory = 6
    }
}
