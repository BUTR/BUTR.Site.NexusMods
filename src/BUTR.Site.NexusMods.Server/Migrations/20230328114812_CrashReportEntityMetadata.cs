using BUTR.Site.NexusMods.Server.Models.Database;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BUTR.Site.NexusMods.Server.Migrations
{
    /// <inheritdoc />
    public partial class CrashReportEntityMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<CrashReportEntityMetadata>(
                name: "metadata",
                table: "crash_report_entity",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "metadata",
                table: "crash_report_entity");
        }
    }
}
