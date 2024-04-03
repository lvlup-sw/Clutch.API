using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StepNet.API.Models.Image;
using StepNet.API.Utilities;

namespace StepNet.API.Database.Context
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