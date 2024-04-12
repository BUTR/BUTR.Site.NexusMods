using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BUTR.Site.NexusMods.Server.Migrations
{
    /// <inheritdoc />
    public partial class ModulesInvolvedIndex2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "index",
                schema: "crashreport",
                table: "crash_report_module_info",
                newName: "involved_position");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "involved_position",
                schema: "crashreport",
                table: "crash_report_module_info",
                newName: "index");
        }
    }
}