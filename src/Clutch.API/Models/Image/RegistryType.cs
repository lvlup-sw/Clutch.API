namespace Clutch.API.Models.Image
{
    public enum RegistryType
    {
        Local = 0,  // GHCR
        Docker = 1,
        Harbor = 2,
        ECR = 3,
        Azure = 4,
        GCR = 5,
        Artifactory = 6
    }
}
