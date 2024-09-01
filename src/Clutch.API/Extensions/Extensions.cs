using Azure.Messaging.ServiceBus;
using StackExchange.Redis;
using Microsoft.OpenApi.Models;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace Clutch.API.Extensions
{
    internal static class Extensions
    {
        public static void AddApplicationServices(this IHostApplicationBuilder builder)
        {
            // Configuration
            builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            // Load in Secrets from Azure
            builder.Configuration.AddAzureKeyVault(
                new SecretClient(
                    new Uri($"https://{builder.Configuration["Azure:AzureKeyVault"]}.vault.azure.net/"),
                    new DefaultAzureCredential()
                ),
                new KeyVaultSecretManager()
            );

            // Bind Secrets to appsettings depending on Env
            if (builder.Environment.IsProduction())
            {
                builder.BindProductionSecrets();
            }
            else
            {
                builder.BindDevelopmentSecrets();
            }

            /* Uncomment to load in App Configuration
             * Note we do not load in secrets here and
             * it is solely used for feature management
            // App Configuration (feature flags)
            builder.Configuration.AddAzureAppConfiguration(options =>
            {
                options.Connect(builder.Configuration.GetConnectionString("AzureAppConfiguration"))
                    .ConfigureKeyVault(kv =>
                    {
                        kv.SetCredential(new DefaultAzureCredential());
                    });
            });
            */

            // Options
            builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
            builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection("CacheSettings"));
            builder.Services.Configure<EventPublisherSettings>(builder.Configuration.GetSection("EventPublisherSettings"));
            builder.Services.AddOptions();

            // Logging
            builder.Services.AddLogging();

            // Redis Client
            builder.AddRedisClient("Redis");

            // Database Context
            builder.AddNpgsqlDbContext<ContainerImageContext>("ContainerImageDb", options =>
            {   // We explicitly handle retries ourselves
                options.DisableRetry = true;
            });

            // Registry Factory & Providers
            builder.Services.AddSingleton<IRestClientFactory, RestClientFactory>();
            builder.Services.AddTransient<IRegistryProvider, RegistryProviderBase>(serviceProvider =>
                new RegistryProviderBase(
                    serviceProvider.GetRequiredService<IRestClientFactory>(),
                    serviceProvider.GetRequiredService<ILogger<RegistryProviderBase>>(),
                    serviceProvider.GetRequiredService<IConfiguration>()
                ));
            builder.Services.AddTransient<IRegistryProvider, DockerRegistryProvider>(serviceProvider =>
                new DockerRegistryProvider(
                    serviceProvider.GetRequiredService<IRestClientFactory>(),
                    serviceProvider.GetRequiredService<ILogger<DockerRegistryProvider>>(),
                    serviceProvider.GetRequiredService<IConfiguration>()
                ));
            builder.Services.AddTransient<IRegistryProvider, AzureRegistryProvider>(serviceProvider =>
                new AzureRegistryProvider(
                    serviceProvider.GetRequiredService<IRestClientFactory>(),
                    serviceProvider.GetRequiredService<ILogger<AzureRegistryProvider>>(),
                    serviceProvider.GetRequiredService<IConfiguration>()
                ));
            builder.Services.AddTransient<IRegistryProviderFactory, RegistryProviderFactory>();

            // Cache Provider
            // Note that we get the IConnectionMultiplexer from the RedisClient DI
            builder.Services.AddTransient<ICacheProvider<ContainerImageModel>>(serviceProvider =>
                new CacheProvider<ContainerImageModel>(
                    serviceProvider.GetRequiredService<IConnectionMultiplexer>(),
                    serviceProvider.GetRequiredService<IContainerImageProvider>(),
                    serviceProvider.GetRequiredService<IOptions<CacheSettings>>(),
                    serviceProvider.GetRequiredService<ILogger<CacheProvider<ContainerImageModel>>>()
                ));

            // Azure Service Bus
            builder.Services.AddSingleton(new ServiceBusClient(builder.Configuration.GetConnectionString("AzureServiceBus")));
            builder.Services.AddSingleton<IEventPublisher, ServiceBusEventPublisher>(serviceProvider =>
                new ServiceBusEventPublisher(
                    serviceProvider.GetRequiredService<ServiceBusClient>(),
                    builder.Configuration["Azure:AzureQueueName"] ?? string.Empty,
                    builder.Configuration["Azure:AzureDLQueueName"] ?? string.Empty,
                    serviceProvider.GetRequiredService<IOptions<EventPublisherSettings>>(),
                    serviceProvider.GetRequiredService<ILogger<ServiceBusEventPublisher>>()
                ));

            // Container Image
            builder.Services.AddTransient<IContainerImageRepository, ContainerImageRepository>(serviceProvider =>
                new ContainerImageRepository(
                    serviceProvider.GetRequiredService<ContainerImageContext>(),
                    serviceProvider.GetRequiredService<ILogger<ContainerImageRepository>>()
                ));
            builder.Services.AddTransient<IContainerImageProvider, ContainerImageProvider>(serviceProvider =>
                new ContainerImageProvider(
                    serviceProvider.GetRequiredService<IContainerImageRepository>(),
                    serviceProvider.GetRequiredService<ILogger<ContainerImageProvider>>(),
                    serviceProvider.GetRequiredService<IOptions<AppSettings>>()
                ));
            builder.Services.AddScoped<IContainerImageService, ContainerImageService>(serviceProvider =>
                new ContainerImageService(
                    serviceProvider.GetRequiredService<ICacheProvider<ContainerImageModel>>(),
                    serviceProvider.GetRequiredService<IContainerImageProvider>(),
                    serviceProvider.GetRequiredService<IRegistryProviderFactory>(),
                    serviceProvider.GetRequiredService<IEventPublisher>(),
                    serviceProvider.GetRequiredService<ILogger<ContainerImageService>>(),
                    serviceProvider.GetRequiredService<IOptions<AppSettings>>()
                ));

            // AutoMapper
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // Swagger
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Clutch API", Version = "v1" });
            });

            // Controllers and Endpoints
            builder.Services.AddControllers()
                .AddJsonOptions(o => o.JsonSerializerOptions.Converters
                    .Add(new JsonStringEnumConverter()));
            builder.Services.AddEndpointsApiExplorer();

            // Healthcheck configuration
            builder.Services.AddRequestTimeouts();
            builder.Services.AddOutputCache();

            // Memory Cache
            builder.Services.AddMemoryCache();
        }

        // Unfortunately we need these extension methods
        // to bind the values retrieved from Azure KeyVault
        // to the ConnectionStrings section of our appsettings
        private static void BindProductionSecrets(this IHostApplicationBuilder builder)
        {
            var connectionStrings = builder.Configuration.GetSection("ConnectionStrings");
            foreach (var secret in connectionStrings.GetChildren())
            {
                if (secret.Key is not "AzureKeyVault")
                    secret.Value = builder.Configuration[secret.Key];
            }
        }

        private static void BindDevelopmentSecrets(this IHostApplicationBuilder builder)
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