using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace StepNet.API.Models.Images
{
    public class ContainerImageModel
    {
        public class ContainerImageContext : DbContext
        {
            public DbSet<DbImage> Images { get; set; }
            /*
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                // Use the .UseMySql() method to configure Pomelo
                optionsBuilder.UseMySql(@"Connection_String",
                    mySqlOptions => mySqlOptions
                        .ServerVersion(new ServerVersion(new Version(8, 0, 27)), ServerType.MySql) // Adjust server version if needed
                );
            }
            */
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
