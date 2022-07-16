using Microsoft.EntityFrameworkCore.Migrations;

using System;

#nullable disable

namespace BUTR.Site.NexusMods.Server.Migrations
{
    public partial class CrashReportIgnoredFilesEntity_Id : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "id",
                table: "crash_report_ignored_files",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "id",
                table: "crash_report_ignored_files");
        }
    }
}