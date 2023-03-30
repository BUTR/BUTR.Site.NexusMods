﻿using Microsoft.EntityFrameworkCore.Migrations;

using System.Collections.Generic;

#nullable disable

namespace BUTR.Site.NexusMods.Server.Migrations
{
    /// <inheritdoc />
    public partial class CrashReportEntityMods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Dictionary<string, string>>(
                name: "mod_id_to_version",
                table: "crash_report_entity",
                type: "hstore",
                nullable: false,
                defaultValue: new Dictionary<string, string>() { { "ignore", "ignore" } });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "mod_id_to_version",
                table: "crash_report_entity");
        }
    }
}