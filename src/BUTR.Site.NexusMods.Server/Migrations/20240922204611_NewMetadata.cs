using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BUTR.Site.NexusMods.Server.Migrations
{
    /// <inheritdoc />
    public partial class NewMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "operating_system_type",
                schema: "crashreport",
                table: "crash_report_metadata",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "operating_system_version",
                schema: "crashreport",
                table: "crash_report_metadata",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_nexusmods_user_role_tenant_tenant",
                schema: "nexusmods_user",
                table: "nexusmods_user_role",
                column: "tenant",
                principalSchema: "tenant",
                principalTable: "tenant",
                principalColumn: "tenant_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_nexusmods_user_role_tenant_tenant",
                schema: "nexusmods_user",
                table: "nexusmods_user_role");

            migrationBuilder.DropColumn(
                name: "operating_system_type",
                schema: "crashreport",
                table: "crash_report_metadata");

            migrationBuilder.DropColumn(
                name: "operating_system_version",
                schema: "crashreport",
                table: "crash_report_metadata");
        }
    }
}
