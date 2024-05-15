using CacheProvider.Providers;
using Clutch.API.Database.Context;
using Clutch.API.Models.Image;
using Clutch.API.Properties;
using Clutch.API.Providers.Image;
using Clutch.API.Providers.Interfaces;
using Clutch.API.Providers.Registry;
using Clutch.API.Repositories.Image;
using Clutch.API.Repositories.Interfaces;
using Clutch.API.Services.Image;
using Clutch.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System.Diagnostics;

namespace Clutch.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebApp(args).Run();
        }

        public static WebApplication CreateWebApp(string[] args)
        {
            // Start local Redis server
            //ExecuteBashScript("../../../Start-Redis.sh");

            // Create the application
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            ConfigureServices(builder);

            // Configure Logging
            ConfigureLogging(builder);

            // Build the application.
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            ConfigureHTTP(app);

            return app;
        }

        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.WebHost.UseKestrel(options => { options.ListenAnyIP(8080); });
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            builder.Configuration.AddEnvironmentVariables();
            builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
            builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection("CacheSettings"));
            builder.Services.AddOptions();
            builder.Services.AddLogging();
            builder.Services.AddSingleton<IRestClientFactory, RestClientFactory>();
            builder.Services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
            {
                return ConnectionMultiplexer.Connect(
                    builder.Configuration.GetConnectionString("Redis")
                    ?? "localhost:6379,abortConnect=false,ssl=false,allowAdmin=true");
            });
            builder.Services.AddDbContext<ContainerImageContext>(options =>
                options.UseMySql(builder.Configuration.GetConnectionString("ClutchAPI"),
                    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("ClutchAPI")))
            , ServiceLifetime.Singleton);
            builder.Services.AddTransient<IContainerImageRepository, ContainerImageRepository>(serviceProvier =>
            {
                return new ContainerImageRepository(
                    serviceProvier.GetRequiredService<ContainerImageContext>(),
                    serviceProvier.GetRequiredService<ILogger<ContainerImageRepository>>()
                );
            });
            builder.Services.AddTransient<IContainerImageProvider, ContainerImageProvider>(serviceProvider =>
            {
                return new ContainerImageProvider(
                    serviceProvider.GetRequiredService<IContainerImageRepository>(),
                    serviceProvider.GetRequiredService<ILogger<ContainerImageProvider>>(),
                    serviceProvider.GetRequiredService<IOptions<AppSettings>>()
                );
            });
            builder.Services.AddTransient<IRegistryProvider, RegistryProviderBase>(serviceProvider =>
            {
                return new RegistryProviderBase(
                    serviceProvider.GetRequiredService<IRestClientFactory>(),
                    serviceProvider.GetRequiredService<ILogger<RegistryProviderBase>>(),
                    serviceProvider.GetRequiredService<IOptions<AppSettings>>()
                );
            });
            builder.Services.AddTransient<IRegistryProvider, DockerRegistryProvider>(serviceProvider =>
            {
                return new DockerRegistryProvider(
                    serviceProvider.GetRequiredService<IRestClientFactory>(),
                    serviceProvider.GetRequiredService<ILogger<DockerRegistryProvider>>(),
                    serviceProvider.GetRequiredService<IOptions<AppSettings>>()
                );
            });
            builder.Services.AddTransient<IRegistryProviderFactory, RegistryProviderFactory>(serviceProvider =>
            {
                return new RegistryProviderFactory(serviceProvider);
            });
            builder.Services.AddTransient<ICacheProvider<ContainerImageModel>>(serviceProvider =>
            {
                return new CacheProvider<ContainerImageModel>(
                    serviceProvider.GetRequiredService<IConnectionMultiplexer>(),
                    serviceProvider.GetRequiredService<IContainerImageProvider>(),
                    serviceProvider.GetRequiredService<IOptions<CacheSettings>>(),
                    serviceProvider.GetRequiredService<ILogger<CacheProvider<ContainerImageModel>>>()
                );
            });
            builder.Services.AddTransient<IContainerImageService, ContainerImageService>(serviceProvider =>
            {
                return new ContainerImageService(
                    serviceProvider.GetRequiredService<ICacheProvider<ContainerImageModel>>(),
                    serviceProvider.GetRequiredService<IContainerImageProvider>(),
                    serviceProvider.GetRequiredService<IRegistryProviderFactory>(),
                    serviceProvider.GetRequiredService<ILogger<ContainerImageService>>(),
                    serviceProvider.GetRequiredService<IOptions<AppSettings>>()
                );
            });
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Clutch API", Version = "v1" });
            });
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddGrpc();
            builder.Services.AddMemoryCache();
        }

        private static void ConfigureLogging(WebApplicationBuilder builder)
        {
            builder.Logging.AddConsole();
            builder.Logging.AddFilter("Microsoft", LogLevel.Information);
            builder.Logging.AddFilter("System", LogLevel.Information);
            builder.Logging.AddFilter("Clutch", LogLevel.Information);
        }

        private static void ConfigureHTTP(WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Clutch.API V1");
            });
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        }

        protected static void ExecuteBashScript(string scriptPath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = @"C:\Program Files\Git\git-bash.exe",
                Arguments = Path.Combine(Directory.GetCurrentDirectory(), scriptPath),
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = false,
                Verb = "runas",
            };

            var process = new Process { StartInfo = startInfo };
            process.Start();
            process.WaitForExit();
        }
    }
}
