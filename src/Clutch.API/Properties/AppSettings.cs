namespace Clutch.API.Properties
{
    public class AppSettings
    {
        public int ProviderRetryCount { get; set; }
        public bool ProviderUseExponentialBackoff { get; set; }
        public int ProviderRetryInterval { get; set; }
        public double CacheExpirationTime { get; set; }
        public double RateLimitReplenishmentPeriod { get; set; }
        public bool RateLimitAutoReplenishment { get; set; }
        public int RateLimitTokenLimit { get; set; }
        public int RateLimitTokensPerPeriod { get; set; }
        public int RateLimitQueueLimit { get; set; }
    }
}
