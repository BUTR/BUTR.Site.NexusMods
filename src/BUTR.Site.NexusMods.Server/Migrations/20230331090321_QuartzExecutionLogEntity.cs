using System;
using BUTR.Site.NexusMods.Server.Models.Database;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BUTR.Site.NexusMods.Server.Migrations
{
    /// <inheritdoc />
    public partial class QuartzExecutionLogEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "quartz_execution_log_entity",
                columns: table => new
                {
                    log_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    run_instance_id = table.Column<string>(type: "text", nullable: true),
                    log_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    job_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    job_group = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    trigger_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    trigger_group = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    _schedule_fire_time_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    fire_time_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    job_run_time = table.Column<TimeSpan>(type: "interval", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: true),
                    result = table.Column<string>(type: "text", nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    is_vetoed = table.Column<bool>(type: "boolean", nullable: true),
                    is_exception = table.Column<bool>(type: "boolean", nullable: true),
                    is_success = table.Column<bool>(type: "boolean", nullable: true),
                    return_code = table.Column<string>(type: "text", nullable: true),
                    date_added_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    execution_log_detail = table.Column<QuartzExecutionLogDetailEntity>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("quartz_execution_log_entity_pkey", x => x.log_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_quartz_execution_log_entity_date_added_utc_log_type",
                table: "quartz_execution_log_entity",
                columns: new[] { "date_added_utc", "log_type" });

            migrationBuilder.CreateIndex(
                name: "IX_quartz_execution_log_entity_run_instance_id",
                table: "quartz_execution_log_entity",
                column: "run_instance_id");

            migrationBuilder.CreateIndex(
                name: "IX_quartz_execution_log_entity_trigger_name_trigger_group_job_~",
                table: "quartz_execution_log_entity",
                columns: new[] { "trigger_name", "trigger_group", "job_name", "job_group", "date_added_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "quartz_execution_log_entity");
        }
    }
}
