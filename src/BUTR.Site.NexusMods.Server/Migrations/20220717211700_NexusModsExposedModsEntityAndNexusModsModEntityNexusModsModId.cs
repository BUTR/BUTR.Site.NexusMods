using Microsoft.EntityFrameworkCore.Migrations;

using System;

#nullable disable

namespace BUTR.Site.NexusMods.Server.Migrations
{
    public partial class NexusModsExposedModsEntityAndNexusModsModEntityNexusModsModId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "mod_id",
                table: "nexusmods_mod_entity",
                newName: "nexusmods_mod_id");

            migrationBuilder.CreateTable(
                name: "nexusmods_exposed_mods_entity",
                columns: table => new
                {
                    nexusmods_mod_id = table.Column<int>(type: "integer", nullable: false),
                    mod_ids = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("nexusmods_exposed_mods_entity_pkey", x => x.nexusmods_mod_id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nexusmods_exposed_mods_entity");

            migrationBuilder.RenameColumn(
                name: "nexusmods_mod_id",
                table: "nexusmods_mod_entity",
                newName: "mod_id");
        }
    }
}