using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BUTR.Site.NexusMods.Server.Migrations
{
    /// <inheritdoc />
    public partial class CrashReportEntityIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "value",
                table: "crash_score_involved_entity",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "value",
                table: "crash_score_involved_entity");
        }
    }
}
