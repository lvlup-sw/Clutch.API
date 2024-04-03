using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Clutch.API.Models.Image;
using Clutch.API.Utilities;

namespace Clutch.API.Database.Context
{
    public class ContainerImageContext(DbContextOptions<ContainerImageContext> options) : DbContext(options)
    {
        public DbSet<ContainerImageModel> ContainerImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var builder = modelBuilder.Entity<ContainerImageModel>();

            builder.ToTable("container_images");
            builder.HasIndex(c => new { c.ImageID, c.ImageReference }).IsUnique();
            builder.Property(c => c.Plugins)
                   .HasConversion(
                       v => JsonConvert.SerializeObject(v),
                       v => JsonConvert.DeserializeObject<List<Plugin>>(v))
                   .Metadata.SetValueComparer(new PluginValueComparer());
        }
    }
}