using Microsoft.EntityFrameworkCore.Migrations;

using System;

#nullable disable

namespace BUTR.Site.NexusMods.Server.Migrations
{
    /// <inheritdoc />
    public partial class NewStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "crash_report_per_day",
                schema: "statistics",
                columns: table => new
                {
                    tenant = table.Column<byte>(type: "smallint", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    game_version = table.Column<string>(type: "text", nullable: false),
                    nexusmods_id = table.Column<int>(type: "integer", nullable: false),
                    module_id = table.Column<string>(type: "text", nullable: false),
                    module_version = table.Column<string>(type: "text", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crash_report_per_day", x => new { x.tenant, x.date, x.game_version, x.nexusmods_id, x.module_id, x.module_version });
                    table.ForeignKey(
                        name: "FK_crash_report_per_day_tenant_tenant",
                        column: x => x.tenant,
                        principalSchema: "tenant",
                        principalTable: "tenant",
                        principalColumn: "tenant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "crash_report_per_month",
                schema: "statistics",
                columns: table => new
                {
                    tenant = table.Column<byte>(type: "smallint", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    game_version = table.Column<string>(type: "text", nullable: false),
                    nexusmods_id = table.Column<int>(type: "integer", nullable: false),
                    module_id = table.Column<string>(type: "text", nullable: false),
                    module_version = table.Column<string>(type: "text", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crash_report_per_month", x => new { x.tenant, x.date, x.game_version, x.nexusmods_id, x.module_id, x.module_version });
                    table.ForeignKey(
                        name: "FK_crash_report_per_month_tenant_tenant",
                        column: x => x.tenant,
                        principalSchema: "tenant",
                        principalTable: "tenant",
                        principalColumn: "tenant_id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "crash_report_per_day",
                schema: "statistics");

            migrationBuilder.DropTable(
                name: "crash_report_per_month",
                schema: "statistics");
        }
    }
}