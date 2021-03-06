using Microsoft.EntityFrameworkCore.Migrations;

using System;

#nullable disable

namespace BUTR.Site.NexusMods.Server.Migrations
{
    public partial class NexusModsExposedModsEntityLastCheckedDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "last_checked_date",
                table: "nexusmods_exposed_mods_entity",
                type: "timestamp with time zone",
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_checked_date",
                table: "nexusmods_exposed_mods_entity");
        }
    }
}