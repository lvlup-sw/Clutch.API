using Microsoft.EntityFrameworkCore;
using Clutch.API.Models.Image;

namespace Clutch.API.Database.Context
{
    public class ContainerImageContext(DbContextOptions<ContainerImageContext> options) : DbContext(options)
    {
        public virtual DbSet<ContainerImageModel> ContainerImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var builder = modelBuilder.Entity<ContainerImageModel>();

            builder.ToTable("container_images");
            builder.HasIndex(c => new { c.ImageID, c.RepositoryId }).IsUnique();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("DataSource=:memory:");
            }
        }
    }
}