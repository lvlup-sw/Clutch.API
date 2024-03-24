using CacheProvider.Providers;
using CacheProvider.Providers.Interfaces;
using StackExchange.Redis;
using System.Diagnostics;
using Microsoft.OpenApi.Models;

namespace StepNet
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
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "StepNet API", Version = "v1" });
            });
            builder.Services.AddControllersWithViews();
            builder.Services.AddGrpc();
            builder.Services.AddOptions();
            builder.Services.AddLogging();
            builder.Services.AddMemoryCache();
            //builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
            builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection("CacheSettings"));
            //builder.Services.AddSingleton<IRealProvider<string>, RealProvider>();
            builder.Services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
            {
                return ConnectionMultiplexer.Connect(
                    builder.Configuration.GetConnectionString("Redis")
                    ?? "localhost:6379,abortConnect=false,ssl=false,allowAdmin=true");
            });
            builder.Services.AddSingleton<ICacheProvider<string>>(serviceProvider =>
            {
                return new CacheProvider<string>(
                    serviceProvider.GetRequiredService<IConnectionMultiplexer>(),
                    serviceProvider.GetRequiredService<IRealProvider<string>>(),
                    serviceProvider.GetRequiredService<CacheSettings>(),
                    serviceProvider.GetRequiredService<ILogger<CacheProvider<string>>>()
                );
            });
        }

        private static void ConfigureLogging(WebApplicationBuilder builder)
        {
            builder.Logging.AddConsole();
            builder.Logging.AddFilter("Microsoft", LogLevel.Information);
            builder.Logging.AddFilter("System", LogLevel.Information);
            builder.Logging.AddFilter("StepNet", LogLevel.Information);
        }

        private static void ConfigureHTTP(WebApplication app)
        {
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.MapGrpcService<KeyValueStoreService>();
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
