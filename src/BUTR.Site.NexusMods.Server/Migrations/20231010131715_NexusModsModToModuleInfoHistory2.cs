using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BUTR.Site.NexusMods.Server.Migrations
{
    /// <inheritdoc />
    public partial class NexusModsModToModuleInfoHistory2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_nexusmods_mod_module_info_history",
                schema: "nexusmods_mod",
                table: "nexusmods_mod_module_info_history");

            migrationBuilder.AddColumn<int>(
                name: "nexusmods_file_id",
                schema: "nexusmods_mod",
                table: "nexusmods_mod_module_info_history",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_of_upload",
                schema: "nexusmods_mod",
                table: "nexusmods_mod_module_info_history",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_nexusmods_mod_module_info_history",
                schema: "nexusmods_mod",
                table: "nexusmods_mod_module_info_history",
                columns: new[] { "tenant", "nexusmods_file_id", "nexusmods_mod_name_id", "module_id", "module_version" });

            migrationBuilder.CreateIndex(
                name: "IX_nexusmods_mod_module_info_history_tenant_nexusmods_mod_name~",
                schema: "nexusmods_mod",
                table: "nexusmods_mod_module_info_history",
                columns: new[] { "tenant", "nexusmods_mod_name_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_nexusmods_mod_module_info_history",
                schema: "nexusmods_mod",
                table: "nexusmods_mod_module_info_history");

            migrationBuilder.DropIndex(
                name: "IX_nexusmods_mod_module_info_history_tenant_nexusmods_mod_name~",
                schema: "nexusmods_mod",
                table: "nexusmods_mod_module_info_history");

            migrationBuilder.DropColumn(
                name: "nexusmods_file_id",
                schema: "nexusmods_mod",
                table: "nexusmods_mod_module_info_history");

            migrationBuilder.DropColumn(
                name: "date_of_upload",
                schema: "nexusmods_mod",
                table: "nexusmods_mod_module_info_history");

            migrationBuilder.AddPrimaryKey(
                name: "PK_nexusmods_mod_module_info_history",
                schema: "nexusmods_mod",
                table: "nexusmods_mod_module_info_history",
                columns: new[] { "tenant", "nexusmods_mod_name_id", "module_id", "module_version" });
        }
    }
}
