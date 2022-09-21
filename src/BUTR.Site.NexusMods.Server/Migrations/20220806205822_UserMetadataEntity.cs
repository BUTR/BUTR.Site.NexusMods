using Microsoft.EntityFrameworkCore.Migrations;

using System.Collections.Generic;

#nullable disable

namespace BUTR.Site.NexusMods.Server.Migrations
{
    public partial class UserMetadataEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:hstore", ",,");

            migrationBuilder.CreateTable(
                name: "user_metadata",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    metadata = table.Column<Dictionary<string, string>>(type: "hstore", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_metadata_pkey", x => x.user_id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_metadata");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:hstore", ",,");
        }
    }
}