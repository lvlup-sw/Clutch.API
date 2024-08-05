using Microsoft.EntityFrameworkCore;

namespace Clutch.API.Database.Context
{
    public class ContainerImageContext(DbContextOptions<ContainerImageContext> options, IConfiguration configuration) : DbContext(options)
    {
        private readonly IConfiguration _configuration = configuration;

        public virtual DbSet<ContainerImageModel> ContainerImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var builder = modelBuilder.Entity<ContainerImageModel>();

            builder.ToTable("container_images");
            builder.HasIndex(c => new { c.ImageID, c.RepositoryId }).IsUnique();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                optionsBuilder.UseInMemoryDatabase("InMemoryDb");
            }
            else
            {
                optionsBuilder.UseNpgsql(_configuration.GetConnectionString("ClutchAPI"));
            }
        }
    }
}