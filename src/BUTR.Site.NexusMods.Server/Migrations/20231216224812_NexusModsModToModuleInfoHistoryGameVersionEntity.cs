using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BUTR.Site.NexusMods.Server.Migrations
{
    /// <inheritdoc />
    public partial class NexusModsModToModuleInfoHistoryGameVersionEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nexusmods_mod_module_info_history_game_version",
                schema: "nexusmods_mod",
                columns: table => new
                {
                    tenant = table.Column<byte>(type: "smallint", nullable: false),
                    module_version = table.Column<string>(type: "text", nullable: false),
                    nexusmods_file_id = table.Column<int>(type: "integer", nullable: false),
                    nexusmods_mod_module_info_history_game_version_id = table.Column<int>(type: "integer", nullable: false),
                    module_id = table.Column<string>(type: "text", nullable: false),
                    game_version = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexusmods_mod_module_info_history_game_version", x => new { x.tenant, x.nexusmods_file_id, x.nexusmods_mod_module_info_history_game_version_id, x.module_id, x.module_version });
                    table.ForeignKey(
                        name: "FK_nexusmods_mod_module_info_history_game_version_module_tenan~",
                        columns: x => new { x.tenant, x.module_id },
                        principalSchema: "module",
                        principalTable: "module",
                        principalColumns: new[] { "tenant", "module_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nexusmods_mod_module_info_history_game_version_nexusmods_mo~",
                        columns: x => new { x.tenant, x.nexusmods_mod_module_info_history_game_version_id },
                        principalSchema: "nexusmods_mod",
                        principalTable: "nexusmods_mod",
                        principalColumns: new[] { "tenant", "nexusmods_mod_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nexusmods_mod_module_info_history_game_version_nexusmods_m~1",
                        columns: x => new { x.tenant, x.nexusmods_file_id, x.nexusmods_mod_module_info_history_game_version_id, x.module_id, x.module_version },
                        principalSchema: "nexusmods_mod",
                        principalTable: "nexusmods_mod_module_info_history",
                        principalColumns: new[] { "tenant", "nexusmods_file_id", "nexusmods_mod_module_info_history_id", "module_id", "module_version" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nexusmods_mod_module_info_history_game_version_tenant_tenant",
                        column: x => x.tenant,
                        principalSchema: "tenant",
                        principalTable: "tenant",
                        principalColumn: "tenant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_nexusmods_mod_module_info_history_game_version_tenant_modul~",
                schema: "nexusmods_mod",
                table: "nexusmods_mod_module_info_history_game_version",
                columns: new[] { "tenant", "module_id" });

            migrationBuilder.CreateIndex(
                name: "IX_nexusmods_mod_module_info_history_game_version_tenant_nexus~",
                schema: "nexusmods_mod",
                table: "nexusmods_mod_module_info_history_game_version",
                columns: new[] { "tenant", "nexusmods_mod_module_info_history_game_version_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nexusmods_mod_module_info_history_game_version",
                schema: "nexusmods_mod");
        }
    }
}
