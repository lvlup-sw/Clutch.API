namespace Clutch.API.Models.Enums
{
    public enum RegistryType
    {
        Invalid = -1,
        Local = 0,  // GHCR
        Docker = 1,
        Google = 2,
        AWS = 3,
        Azure = 4,
        Artifactory = 5
    }
}
