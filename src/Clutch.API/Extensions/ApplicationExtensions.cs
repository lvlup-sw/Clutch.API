using System.Security.Claims;
using System.Threading.RateLimiting;

namespace Clutch.API.Extensions
{
    internal static class EndpointExtensions
    {
        private static readonly string Policy = "PerUserRatelimit";

        public static IServiceCollection AddRateLimiting(this IServiceCollection services)
        {
            // Get AppSettings
            var settings = services.BuildServiceProvider().GetRequiredService<IOptions<AppSettings>>().Value;

            return services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.AddPolicy(Policy, context =>
                {
                    // We always have a user name
                    var username = context.User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                    return RateLimitPartition.GetTokenBucketLimiter(username, key =>
                    {
                        return new()
                        {   // This will be used for a mobile app, so it's best to be generous
                            ReplenishmentPeriod =   TimeSpan.FromSeconds(settings.RateLimitReplenishmentPeriod),
                            AutoReplenishment =     settings.RateLimitAutoReplenishment,
                            TokenLimit =            settings.RateLimitTokenLimit,
                            TokensPerPeriod =       settings.RateLimitTokensPerPeriod,
                            QueueLimit =            settings.RateLimitQueueLimit,
                        };
                    });
                });
            });
        }

        public static IEndpointConventionBuilder RequirePerUserRateLimit(this IEndpointConventionBuilder builder)
        {
            return builder.RequireRateLimiting(Policy);
        }
    }
}
