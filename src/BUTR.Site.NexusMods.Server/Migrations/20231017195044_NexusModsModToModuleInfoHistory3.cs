using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BUTR.Site.NexusMods.Server.Migrations
{
    /// <inheritdoc />
    public partial class NexusModsModToModuleInfoHistory3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "nexusmods_mod_name_id",
                schema: "nexusmods_mod",
                table: "nexusmods_mod_module_info_history",
                newName: "nexusmods_mod_module_info_history_id");

            migrationBuilder.RenameIndex(
                name: "IX_nexusmods_mod_module_info_history_tenant_nexusmods_mod_name~",
                schema: "nexusmods_mod",
                table: "nexusmods_mod_module_info_history",
                newName: "IX_nexusmods_mod_module_info_history_tenant_nexusmods_mod_modu~");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "nexusmods_mod_module_info_history_id",
                schema: "nexusmods_mod",
                table: "nexusmods_mod_module_info_history",
                newName: "nexusmods_mod_name_id");

            migrationBuilder.RenameIndex(
                name: "IX_nexusmods_mod_module_info_history_tenant_nexusmods_mod_modu~",
                schema: "nexusmods_mod",
                table: "nexusmods_mod_module_info_history",
                newName: "IX_nexusmods_mod_module_info_history_tenant_nexusmods_mod_name~");
        }
    }
}