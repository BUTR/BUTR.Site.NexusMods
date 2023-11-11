using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore.Migrations;

using System;

#nullable disable

namespace BUTR.Site.NexusMods.Server.Migrations
{
    /// <inheritdoc />
    public partial class Quartz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "quartz");

            migrationBuilder.CreateSequence<int>(
                name: "quartz_log_id_seq",
                schema: "quartz");

            migrationBuilder.CreateTable(
                name: "quartz_log",
                schema: "quartz",
                columns: table => new
                {
                    run_instance_id = table.Column<string>(type: "text", nullable: false),
                    job_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    job_group = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    trigger_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    trigger_group = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    fire_time_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    quartz_log_id = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "nextval('\"quartz\".\"quartz_log_id_seq\"')"),
                    job_run_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    schedule_fire_time_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    is_success = table.Column<bool>(type: "boolean", nullable: true),
                    is_exception = table.Column<bool>(type: "boolean", nullable: true),
                    is_vetoed = table.Column<bool>(type: "boolean", nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    log_detail = table.Column<QuartzExecutionLogDetailEntity>(type: "jsonb", nullable: true),
                    result = table.Column<string>(type: "text", nullable: true),
                    return_code = table.Column<string>(type: "text", nullable: true),
                    date_added_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    machie_name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quartz_log", x => new { x.run_instance_id, x.job_name, x.job_group, x.trigger_name, x.trigger_group, x.fire_time_utc });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "quartz_log",
                schema: "quartz");

            migrationBuilder.DropSequence(
                name: "quartz_log_id_seq",
                schema: "quartz");
        }
    }
}