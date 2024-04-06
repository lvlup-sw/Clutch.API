namespace Clutch.API.Properties
{
    public class AppSettings
    {
        public int ProviderRetryCount { get; set; }
        public bool ProviderUseExponentialBackoff { get; set; }
        public int ProviderRetryInterval { get; set; }
        public double CacheExpirationTime { get; set; }
        public string? GithubPAT { get; set; }
    }
}
