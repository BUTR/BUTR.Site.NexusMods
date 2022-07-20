using Microsoft.EntityFrameworkCore.Migrations;

using System;

#nullable disable

namespace BUTR.Site.NexusMods.Server.Migrations
{
    public partial class NexusModsFileUpdateEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nexusmods_file_update_entity",
                columns: table => new
                {
                    nexusmods_mod_id = table.Column<int>(type: "integer", nullable: false),
                    last_checked_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("nexusmods_file_update_entity_pkey", x => x.nexusmods_mod_id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nexusmods_file_update_entity");
        }
    }
}