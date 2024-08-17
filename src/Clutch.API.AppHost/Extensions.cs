namespace Clutch.API.AppHost
{
    internal static class Extensions
    {
        // AFAIK there is no other easier/cleaner way to
        // bind secrets to my appsettings ConnectionStrings
        // since Azure KeyVault entries are imported as-is:
        // key-value pairs, e.g. "AzureClientSecret". So I
        // have to directly bind the entries to the section
        // I want. If KeyVault allowed for non-alphanumeric
        // characters (':'), then I could bind them implicitly
        // as a natural result of the import.

        public static void BindProductionSecrets(this IDistributedApplicationBuilder builder)
        {
            var connectionStrings = builder.Configuration.GetSection("ConnectionStrings");
            foreach (var secret in connectionStrings.GetChildren())
            {
                if (secret.Key is not "AzureKeyVault")
                    secret.Value = builder.Configuration[secret.Key];
            }
        }

        public static void BindDevelopmentSecrets(this IDistributedApplicationBuilder builder)
        {
            var connectionStrings = builder.Configuration.GetSection("ConnectionStrings");
            foreach (var secret in connectionStrings.GetChildren())
            {
                if (secret.Key is not "AzureKeyVault"
                    && secret.Key is not "ContainerImageDb"
                    && secret.Key is not "Redis")
                {
                    secret.Value = builder.Configuration[secret.Key];
                }
            }
        }
    }
}