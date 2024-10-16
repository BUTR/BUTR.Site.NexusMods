using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BUTR.Site.NexusMods.Server.Migrations
{
    /// <inheritdoc />
    public partial class SteamWorkshop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "steamworkshop_mod");

            migrationBuilder.RenameColumn(
                name: "nexusmods_user_nexusmods_mod_link_type_id",
                schema: "nexusmods_user",
                table: "nexusmods_user_nexusmods_mod",
                newName: "nexusmods_user_mod_link_type_id");

            migrationBuilder.RenameColumn(
                name: "nexusmods_mod_module_link_type_id",
                schema: "nexusmods_mod",
                table: "nexusmods_mod_module",
                newName: "mod_module_link_type_id");

            migrationBuilder.AddColumn<int>(
                name: "steamworkshop_mod_id",
                schema: "crashreport",
                table: "crash_report_module_info",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "steamworkshop_mod",
                schema: "steamworkshop_mod",
                columns: table => new
                {
                    tenant = table.Column<byte>(type: "smallint", nullable: false),
                    steamworkshop_mod_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_steamworkshop_mod", x => new { x.tenant, x.steamworkshop_mod_id });
                    table.ForeignKey(
                        name: "FK_steamworkshop_mod_tenant_tenant",
                        column: x => x.tenant,
                        principalSchema: "tenant",
                        principalTable: "tenant",
                        principalColumn: "tenant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nexusmods_user_steamworkshop_mod",
                schema: "nexusmods_user",
                columns: table => new
                {
                    tenant = table.Column<byte>(type: "smallint", nullable: false),
                    nexusmods_user_steamworkshop_mod_id = table.Column<int>(type: "integer", nullable: false),
                    steamworkshop_mod_id = table.Column<int>(type: "integer", nullable: false),
                    nexusmods_user_mod_link_type_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexusmods_user_steamworkshop_mod", x => new { x.tenant, x.nexusmods_user_steamworkshop_mod_id, x.steamworkshop_mod_id, x.nexusmods_user_mod_link_type_id });
                    table.ForeignKey(
                        name: "FK_nexusmods_user_steamworkshop_mod_nexusmods_user_nexusmods_u~",
                        column: x => x.nexusmods_user_steamworkshop_mod_id,
                        principalSchema: "nexusmods_user",
                        principalTable: "nexusmods_user",
                        principalColumn: "nexusmods_user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nexusmods_user_steamworkshop_mod_steamworkshop_mod_tenant_s~",
                        columns: x => new { x.tenant, x.steamworkshop_mod_id },
                        principalSchema: "steamworkshop_mod",
                        principalTable: "steamworkshop_mod",
                        principalColumns: new[] { "tenant", "steamworkshop_mod_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nexusmods_user_steamworkshop_mod_tenant_tenant",
                        column: x => x.tenant,
                        principalSchema: "tenant",
                        principalTable: "tenant",
                        principalColumn: "tenant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "steamworkshop_mod_file_update",
                schema: "steamworkshop_mod",
                columns: table => new
                {
                    tenant = table.Column<byte>(type: "smallint", nullable: false),
                    steamworkshop_mod_file_update_id = table.Column<int>(type: "integer", nullable: false),
                    date_of_last_check = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_steamworkshop_mod_file_update", x => new { x.tenant, x.steamworkshop_mod_file_update_id });
                    table.ForeignKey(
                        name: "FK_steamworkshop_mod_file_update_steamworkshop_mod_tenant_stea~",
                        columns: x => new { x.tenant, x.steamworkshop_mod_file_update_id },
                        principalSchema: "steamworkshop_mod",
                        principalTable: "steamworkshop_mod",
                        principalColumns: new[] { "tenant", "steamworkshop_mod_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_steamworkshop_mod_file_update_tenant_tenant",
                        column: x => x.tenant,
                        principalSchema: "tenant",
                        principalTable: "tenant",
                        principalColumn: "tenant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "steamworkshop_mod_module",
                schema: "steamworkshop_mod",
                columns: table => new
                {
                    tenant = table.Column<byte>(type: "smallint", nullable: false),
                    steamworkshop_mod_module_id = table.Column<int>(type: "integer", nullable: false),
                    module_id = table.Column<string>(type: "text", nullable: false),
                    mod_module_link_type_id = table.Column<int>(type: "integer", nullable: false),
                    date_of_last_update = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_steamworkshop_mod_module", x => new { x.tenant, x.steamworkshop_mod_module_id, x.module_id, x.mod_module_link_type_id });
                    table.ForeignKey(
                        name: "FK_steamworkshop_mod_module_module_tenant_module_id",
                        columns: x => new { x.tenant, x.module_id },
                        principalSchema: "module",
                        principalTable: "module",
                        principalColumns: new[] { "tenant", "module_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_steamworkshop_mod_module_steamworkshop_mod_tenant_steamwork~",
                        columns: x => new { x.tenant, x.steamworkshop_mod_module_id },
                        principalSchema: "steamworkshop_mod",
                        principalTable: "steamworkshop_mod",
                        principalColumns: new[] { "tenant", "steamworkshop_mod_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_steamworkshop_mod_module_tenant_tenant",
                        column: x => x.tenant,
                        principalSchema: "tenant",
                        principalTable: "tenant",
                        principalColumn: "tenant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "steamworkshop_mod_name",
                schema: "steamworkshop_mod",
                columns: table => new
                {
                    tenant = table.Column<byte>(type: "smallint", nullable: false),
                    steamworkshop_mod_name_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_steamworkshop_mod_name", x => new { x.tenant, x.steamworkshop_mod_name_id });
                    table.ForeignKey(
                        name: "FK_steamworkshop_mod_name_steamworkshop_mod_tenant_steamworksh~",
                        columns: x => new { x.tenant, x.steamworkshop_mod_name_id },
                        principalSchema: "steamworkshop_mod",
                        principalTable: "steamworkshop_mod",
                        principalColumns: new[] { "tenant", "steamworkshop_mod_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_steamworkshop_mod_name_tenant_tenant",
                        column: x => x.tenant,
                        principalSchema: "tenant",
                        principalTable: "tenant",
                        principalColumn: "tenant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_crash_report_module_info_tenant_steamworkshop_mod_id",
                schema: "crashreport",
                table: "crash_report_module_info",
                columns: new[] { "tenant", "steamworkshop_mod_id" });

            migrationBuilder.CreateIndex(
                name: "IX_nexusmods_user_steamworkshop_mod_nexusmods_user_steamworksh~",
                schema: "nexusmods_user",
                table: "nexusmods_user_steamworkshop_mod",
                column: "nexusmods_user_steamworkshop_mod_id");

            migrationBuilder.CreateIndex(
                name: "IX_nexusmods_user_steamworkshop_mod_tenant_steamworkshop_mod_id",
                schema: "nexusmods_user",
                table: "nexusmods_user_steamworkshop_mod",
                columns: new[] { "tenant", "steamworkshop_mod_id" });

            migrationBuilder.CreateIndex(
                name: "IX_steamworkshop_mod_module_tenant_module_id",
                schema: "steamworkshop_mod",
                table: "steamworkshop_mod_module",
                columns: new[] { "tenant", "module_id" });

            migrationBuilder.AddForeignKey(
                name: "FK_crash_report_module_info_steamworkshop_mod_tenant_steamwork~",
                schema: "crashreport",
                table: "crash_report_module_info",
                columns: new[] { "tenant", "steamworkshop_mod_id" },
                principalSchema: "steamworkshop_mod",
                principalTable: "steamworkshop_mod",
                principalColumns: new[] { "tenant", "steamworkshop_mod_id" },
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_crash_report_module_info_steamworkshop_mod_tenant_steamwork~",
                schema: "crashreport",
                table: "crash_report_module_info");

            migrationBuilder.DropTable(
                name: "nexusmods_user_steamworkshop_mod",
                schema: "nexusmods_user");

            migrationBuilder.DropTable(
                name: "steamworkshop_mod_file_update",
                schema: "steamworkshop_mod");

            migrationBuilder.DropTable(
                name: "steamworkshop_mod_module",
                schema: "steamworkshop_mod");

            migrationBuilder.DropTable(
                name: "steamworkshop_mod_name",
                schema: "steamworkshop_mod");

            migrationBuilder.DropTable(
                name: "steamworkshop_mod",
                schema: "steamworkshop_mod");

            migrationBuilder.DropIndex(
                name: "IX_crash_report_module_info_tenant_steamworkshop_mod_id",
                schema: "crashreport",
                table: "crash_report_module_info");

            migrationBuilder.DropColumn(
                name: "steamworkshop_mod_id",
                schema: "crashreport",
                table: "crash_report_module_info");

            migrationBuilder.RenameColumn(
                name: "nexusmods_user_mod_link_type_id",
                schema: "nexusmods_user",
                table: "nexusmods_user_nexusmods_mod",
                newName: "nexusmods_user_nexusmods_mod_link_type_id");

            migrationBuilder.RenameColumn(
                name: "mod_module_link_type_id",
                schema: "nexusmods_mod",
                table: "nexusmods_mod_module",
                newName: "nexusmods_mod_module_link_type_id");
        }
    }
}
