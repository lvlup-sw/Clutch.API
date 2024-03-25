using Microsoft.EntityFrameworkCore;

namespace StepNet.API.Models.Image
{
    public class ContainerImageContext : DbContext
    {
        public ContainerImageContext(DbContextOptions<ContainerImageContext> options) : base(options) { }

        public DbSet<ContainerImageModel> ContainerImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ContainerImageModel>()
                        .ToTable("container_images")
                        .HasIndex(c => c.ImageID)
                        .IsUnique();
        }
    }
}