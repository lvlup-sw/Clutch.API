using Microsoft.EntityFrameworkCore;

namespace StepNet.API.Models.Images
{
    public class ContainerImageModel
    {
        public class ContainerImageContext : DbContext
        {
            public DbSet<DbImage> Images { get; set; }
            // ... (Constructor for dependency injection)
        }

        public class DbImage
        {
            public int Image_ID { get; set; }
            public required string Unique_Identifier { get; set; }
            public required string Image_Name { get; set; }
            public string? Image_Tag { get; set; }
        }
    }
}
