using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BUTR.Site.NexusMods.Server.Migrations
{
    /// <inheritdoc />
    public partial class SteamWorkshop2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "steamworkshop_mod_name_id",
                schema: "steamworkshop_mod",
                table: "steamworkshop_mod_name",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "steamworkshop_mod_module_id",
                schema: "steamworkshop_mod",
                table: "steamworkshop_mod_module",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "steamworkshop_mod_file_update_id",
                schema: "steamworkshop_mod",
                table: "steamworkshop_mod_file_update",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "steamworkshop_mod_id",
                schema: "steamworkshop_mod",
                table: "steamworkshop_mod",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "steamworkshop_mod_id",
                schema: "nexusmods_user",
                table: "nexusmods_user_steamworkshop_mod",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "steamworkshop_mod_id",
                schema: "crashreport",
                table: "crash_report_module_info",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "steamworkshop_mod_name_id",
                schema: "steamworkshop_mod",
                table: "steamworkshop_mod_name",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "steamworkshop_mod_module_id",
                schema: "steamworkshop_mod",
                table: "steamworkshop_mod_module",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "steamworkshop_mod_file_update_id",
                schema: "steamworkshop_mod",
                table: "steamworkshop_mod_file_update",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "steamworkshop_mod_id",
                schema: "steamworkshop_mod",
                table: "steamworkshop_mod",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "steamworkshop_mod_id",
                schema: "nexusmods_user",
                table: "nexusmods_user_steamworkshop_mod",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "steamworkshop_mod_id",
                schema: "crashreport",
                table: "crash_report_module_info",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
