using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Clutch.API.Migrations
{
    /// <inheritdoc />
    public partial class container_images : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "container_images",
                columns: table => new
                {
                    ImageID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RepositoryId = table.Column<string>(type: "character varying(384)", maxLength: 384, nullable: false),
                    Repository = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Tag = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    BuildDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RegistryType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_container_images", x => x.ImageID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_container_images_ImageID_RepositoryId",
                table: "container_images",
                columns: new[] { "ImageID", "RepositoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_container_images_RepositoryId",
                table: "container_images",
                column: "RepositoryId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "container_images");
        }
    }
}
