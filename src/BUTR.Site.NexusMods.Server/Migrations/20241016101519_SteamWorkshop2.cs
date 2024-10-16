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
            migrationBuilder.DropForeignKey(
                name: "FK_steamworkshop_mod_file_update_steamworkshop_mod_tenant_stea~",
                table: "steamworkshop_mod_file_update",
                schema: "steamworkshop_mod");

            migrationBuilder.DropForeignKey(
                name: "FK_steamworkshop_mod_module_steamworkshop_mod_tenant_steamwork~",
                table: "steamworkshop_mod_module",
                schema: "steamworkshop_mod");

            migrationBuilder.DropForeignKey(
                name: "FK_steamworkshop_mod_name_steamworkshop_mod_tenant_steamworksh~",
                table: "steamworkshop_mod_name",
                schema: "steamworkshop_mod");

            migrationBuilder.DropForeignKey(
                name: "FK_nexusmods_user_steamworkshop_mod_steamworkshop_mod_tenant_s~",
                table: "nexusmods_user_steamworkshop_mod",
                schema: "nexusmods_user");

            migrationBuilder.DropForeignKey(
                name: "FK_crash_report_module_info_steamworkshop_mod_tenant_steamwork~",
                table: "crash_report_module_info",
                schema: "crashreport");


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


            migrationBuilder.AddForeignKey(
                name: "FK_steamworkshop_mod_file_update_steamworkshop_mod_tenant_stea~",
                schema: "steamworkshop_mod",
                table: "steamworkshop_mod_file_update",
                columns: ["tenant", "steamworkshop_mod_file_update_id"],
                principalSchema: "steamworkshop_mod",
                principalTable: "steamworkshop_mod",
                principalColumns: ["tenant", "steamworkshop_mod_id"],
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_steamworkshop_mod_module_steamworkshop_mod_tenant_steamwork~",
                schema: "steamworkshop_mod",
                table: "steamworkshop_mod_module",
                columns: ["tenant", "steamworkshop_mod_module_id"],
                principalSchema: "steamworkshop_mod",
                principalTable: "steamworkshop_mod",
                principalColumns: ["tenant", "steamworkshop_mod_id"],
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_steamworkshop_mod_name_steamworkshop_mod_tenant_steamworksh~",
                schema: "steamworkshop_mod",
                table: "steamworkshop_mod_name",
                columns: ["tenant", "steamworkshop_mod_name_id"],
                principalSchema: "steamworkshop_mod",
                principalTable: "steamworkshop_mod",
                principalColumns: ["tenant", "steamworkshop_mod_id"],
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_nexusmods_user_steamworkshop_mod_steamworkshop_mod_tenant_s~",
                schema: "nexusmods_user",
                table: "nexusmods_user_steamworkshop_mod",
                columns: ["tenant", "steamworkshop_mod_id"],
                principalSchema: "steamworkshop_mod",
                principalTable: "steamworkshop_mod",
                principalColumns: ["tenant", "steamworkshop_mod_id"],
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_crash_report_module_info_steamworkshop_mod_tenant_steamwork~",
                schema: "crashreport",
                table: "crash_report_module_info",
                columns: ["tenant", "steamworkshop_mod_id"],
                principalSchema: "steamworkshop_mod",
                principalTable: "steamworkshop_mod",
                principalColumns: ["tenant", "steamworkshop_mod_id"],
                onDelete: ReferentialAction.SetNull);
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