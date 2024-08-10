using Azure.Messaging.ServiceBus;
using StackExchange.Redis;
using Microsoft.OpenApi.Models;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Extensions.AspNetCore.Configuration.Secrets;

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
                    new Uri($"https://{builder.Configuration["AzureKeyVault"]}.vault.azure.net/"),
                    new DefaultAzureCredential()
                ),
                new KeyVaultSecretManager()
            );

            builder.Configuration.AddAzureAppConfiguration(options =>
            {
                options.Connect(builder.Configuration.GetConnectionString("AzureAppConfiguration"))
                    .ConfigureKeyVault(kv =>
                    {
                        kv.SetCredential(new DefaultAzureCredential());

                    });
            });

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
            builder.AddNpgsqlDbContext<ContainerImageContext>("containerImageDb", options =>
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
                    builder.Configuration["AzureQueueName"] ?? string.Empty,
                    builder.Configuration["AzureDLQueueName"] ?? string.Empty,
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
            builder.Services.AddTransient<IContainerImageService, ContainerImageService>(serviceProvider =>
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
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // Memory Cache
            builder.Services.AddMemoryCache();
        }

        public static void AddApplicationLogging(this WebApplicationBuilder builder)
        {
            // Refactor to configure OpenTelemetry with Aspire
            // since OpenTelemetry is natively supported, we just
            // need to consider the structure of the logs before
            // exporting them to Azure Monitor Application Insights
            // We should also be adding filters based on Env; ie
            // debug level for Dev, and information level for prod/qa
            builder.Logging.AddConsole();
            builder.Logging.AddFilter("Microsoft", LogLevel.Information);
            builder.Logging.AddFilter("System", LogLevel.Information);
            builder.Logging.AddFilter("Clutch", LogLevel.Information);
        }
    }
}