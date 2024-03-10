using CacheProvider.Providers;
using CacheProvider.Providers.Interfaces;
using StackExchange.Redis;
using System.Diagnostics;

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
            //ExecuteBashScript("../../../Start-Redis.sh");
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
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

            // Configure Logging
            builder.Logging.AddConsole();
            builder.Logging.AddFilter("Microsoft", LogLevel.Information);
            builder.Logging.AddFilter("System", LogLevel.Information);
            builder.Logging.AddFilter("StepNet", LogLevel.Information);

            // Build the application.
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Configure the HTTP request pipeline.
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            return app;
        }

        public static void ExecuteBashScript(string scriptPath)
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
