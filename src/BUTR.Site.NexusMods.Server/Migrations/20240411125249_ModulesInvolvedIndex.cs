using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BUTR.Site.NexusMods.Server.Migrations
{
    /// <inheritdoc />
    public partial class ModulesInvolvedIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "index",
                schema: "crashreport",
                table: "crash_report_module_info",
                type: "smallint",
                nullable: false,
                defaultValue: (byte) 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "index",
                schema: "crashreport",
                table: "crash_report_module_info");
        }
    }
}