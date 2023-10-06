using BUTR.Site.NexusMods.Server.Models.Database;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BUTR.Site.NexusMods.Server.Migrations
{
    /// <inheritdoc />
    public partial class NexusModsModToModuleInfoHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nexusmods_mod_module_info_history",
                schema: "nexusmods_mod",
                columns: table => new
                {
                    tenant = table.Column<byte>(type: "smallint", nullable: false),
                    module_version = table.Column<string>(type: "text", nullable: false),
                    nexusmods_mod_name_id = table.Column<int>(type: "integer", nullable: false),
                    module_id = table.Column<string>(type: "text", nullable: false),
                    module_info = table.Column<ModuleInfoModel>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexusmods_mod_module_info_history", x => new { x.tenant, x.nexusmods_mod_name_id, x.module_id, x.module_version });
                    table.ForeignKey(
                        name: "FK_nexusmods_mod_module_info_history_module_tenant_module_id",
                        columns: x => new { x.tenant, x.module_id },
                        principalSchema: "module",
                        principalTable: "module",
                        principalColumns: new[] { "tenant", "module_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nexusmods_mod_module_info_history_nexusmods_mod_tenant_nexu~",
                        columns: x => new { x.tenant, x.nexusmods_mod_name_id },
                        principalSchema: "nexusmods_mod",
                        principalTable: "nexusmods_mod",
                        principalColumns: new[] { "tenant", "nexusmods_mod_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nexusmods_mod_module_info_history_tenant_tenant",
                        column: x => x.tenant,
                        principalSchema: "tenant",
                        principalTable: "tenant",
                        principalColumn: "tenant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_nexusmods_mod_module_info_history_tenant_module_id",
                schema: "nexusmods_mod",
                table: "nexusmods_mod_module_info_history",
                columns: new[] { "tenant", "module_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nexusmods_mod_module_info_history",
                schema: "nexusmods_mod");
        }
    }
}
