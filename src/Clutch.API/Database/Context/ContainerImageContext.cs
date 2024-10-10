using Microsoft.EntityFrameworkCore;

namespace Clutch.API.Database.Context
{
    public class ContainerImageContext(DbContextOptions<ContainerImageContext> options) : DbContext(options)
    {
        public virtual DbSet<ContainerImageModel> ContainerImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ContainerImageModel>()
                .ToTable("container_images")
                .HasIndex(c => new { c.ImageID, c.RepositoryId })
                .IsUnique();
        }
    }
}