using Microsoft.EntityFrameworkCore.Migrations;

using System;
using System.Collections.Generic;

#nullable disable

namespace BUTR.Site.NexusMods.Server.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "crash_report_entity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_version = table.Column<string>(type: "text", nullable: false),
                    exception = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    mod_ids = table.Column<List<string>>(type: "text[]", nullable: false),
                    involved_mod_ids = table.Column<List<string>>(type: "text[]", nullable: false),
                    mod_nexusmods_ids = table.Column<List<int>>(type: "integer[]", nullable: false),
                    url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("crash_report_entity_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mod_nexusmods_manual_link",
                columns: table => new
                {
                    mod_id = table.Column<string>(type: "text", nullable: false),
                    nexusmods_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("mod_nexus_mods_manual_link_pkey", x => x.mod_id);
                });

            migrationBuilder.CreateTable(
                name: "nexusmods_mod_entity",
                columns: table => new
                {
                    mod_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    user_ids = table.Column<List<int>>(type: "integer[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("nexusmods_mod_entity_pkey", x => x.mod_id);
                });

            migrationBuilder.CreateTable(
                name: "user_allowed_mods",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    allowed_mod_ids = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_allowed_mods_pkey", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "user_role",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_role_pkey", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "crash_report_file",
                columns: table => new
                {
                    filename = table.Column<string>(type: "text", nullable: false),
                    crash_report_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("crash_report_file_entity_pkey", x => x.filename);
                    table.ForeignKey(
                        name: "FK_crash_report_file_entity_crash_report_id",
                        column: x => x.crash_report_id,
                        principalTable: "crash_report_entity",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_crash_report_entity",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    crash_report_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_crash_report_entity_pkey", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_user_crash_report_entity_crash_report_entity_crash_report_id",
                        column: x => x.crash_report_id,
                        principalTable: "crash_report_entity",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_crash_report_file_crash_report_id",
                table: "crash_report_file",
                column: "crash_report_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_crash_report_entity_crash_report_id",
                table: "user_crash_report_entity",
                column: "crash_report_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "crash_report_file");

            migrationBuilder.DropTable(
                name: "mod_nexusmods_manual_link");

            migrationBuilder.DropTable(
                name: "nexusmods_mod_entity");

            migrationBuilder.DropTable(
                name: "user_allowed_mods");

            migrationBuilder.DropTable(
                name: "user_crash_report_entity");

            migrationBuilder.DropTable(
                name: "user_role");

            migrationBuilder.DropTable(
                name: "crash_report_entity");
        }
    }
}