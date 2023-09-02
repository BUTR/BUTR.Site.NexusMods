using System;
using System.Collections.Generic;
using BUTR.Site.NexusMods.Server.Models.Database;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BUTR.Site.NexusMods.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "autocomplete");

            migrationBuilder.EnsureSchema(
                name: "crashreport");

            migrationBuilder.EnsureSchema(
                name: "statistics");

            migrationBuilder.EnsureSchema(
                name: "exception");

            migrationBuilder.EnsureSchema(
                name: "integration");

            migrationBuilder.EnsureSchema(
                name: "module");

            migrationBuilder.EnsureSchema(
                name: "nexusmods_article");

            migrationBuilder.EnsureSchema(
                name: "nexusmods_mod");

            migrationBuilder.EnsureSchema(
                name: "nexusmods_user");

            migrationBuilder.EnsureSchema(
                name: "quartz");

            migrationBuilder.EnsureSchema(
                name: "tenant");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:hstore", ",,");

            migrationBuilder.CreateTable(
                name: "nexusmods_user",
                schema: "nexusmods_user",
                columns: table => new
                {
                    nexusmods_user_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexusmods_user", x => x.nexusmods_user_id);
                });

            migrationBuilder.CreateTable(
                name: "quartz_execution_log",
                schema: "quartz",
                columns: table => new
                {
                    quartz_execution_log_id = table.Column<long>(type: "bigint", nullable: false)
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
                    execution_log_detail = table.Column<QuartzExecutionLogDetailEntity>(type: "jsonb", nullable: true),
                    machie_name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quartz_execution_log", x => x.quartz_execution_log_id);
                });

            migrationBuilder.CreateTable(
                name: "tenant",
                schema: "tenant",
                columns: table => new
                {
                    tenant_id = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant", x => x.tenant_id);
                });

            migrationBuilder.CreateTable(
                name: "nexusmods_user_name",
                schema: "nexusmods_user",
                columns: table => new
                {
                    nexusmods_user_name_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexusmods_user_name", x => x.nexusmods_user_name_id);
                    table.ForeignKey(
                        name: "FK_nexusmods_user_name_nexusmods_user_nexusmods_user_name_id",
                        column: x => x.nexusmods_user_name_id,
                        principalSchema: "nexusmods_user",
                        principalTable: "nexusmods_user",
                        principalColumn: "nexusmods_user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nexusmods_user_role",
                schema: "nexusmods_user",
                columns: table => new
                {
                    tenant = table.Column<byte>(type: "smallint", nullable: false),
                    nexusmods_user_role_id = table.Column<int>(type: "integer", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexusmods_user_role", x => new { x.tenant, x.nexusmods_user_role_id });
                    table.ForeignKey(
                        name: "FK_nexusmods_user_role_nexusmods_user_nexusmods_user_role_id",
                        column: x => x.nexusmods_user_role_id,
                        principalSchema: "nexusmods_user",
                        principalTable: "nexusmods_user",
                        principalColumn: "nexusmods_user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nexusmods_user_to_integration_discord",
                schema: "nexusmods_user",
                columns: table => new
                {
                    nexusmods_user_to_discord_id = table.Column<int>(type: "integer", nullable: false),
                    discord_user_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexusmods_user_to_integration_discord", x => x.nexusmods_user_to_discord_id);
                    table.UniqueConstraint("AK_nexusmods_user_to_integration_discord_discord_user_id", x => x.discord_user_id);
                    table.ForeignKey(
                        name: "FK_nexusmods_user_to_integration_discord_nexusmods_user_nexusm~",
                        column: x => x.nexusmods_user_to_discord_id,
                        principalSchema: "nexusmods_user",
                        principalTable: "nexusmods_user",
                        principalColumn: "nexusmods_user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nexusmods_user_to_integration_gog",
                schema: "nexusmods_user",
                columns: table => new
                {
                    nexusmods_user_to_gog_id = table.Column<int>(type: "integer", nullable: false),
                    gog_user_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexusmods_user_to_integration_gog", x => x.nexusmods_user_to_gog_id);
                    table.UniqueConstraint("AK_nexusmods_user_to_integration_gog_gog_user_id", x => x.gog_user_id);
                    table.ForeignKey(
                        name: "FK_nexusmods_user_to_integration_gog_nexusmods_user_nexusmods_~",
                        column: x => x.nexusmods_user_to_gog_id,
                        principalSchema: "nexusmods_user",
                        principalTable: "nexusmods_user",
                        principalColumn: "nexusmods_user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nexusmods_user_to_integration_steam",
                schema: "nexusmods_user",
                columns: table => new
                {
                    nexusmods_user_to_steam_id = table.Column<int>(type: "integer", nullable: false),
                    steam_user_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexusmods_user_to_integration_steam", x => x.nexusmods_user_to_steam_id);
                    table.UniqueConstraint("AK_nexusmods_user_to_integration_steam_steam_user_id", x => x.steam_user_id);
                    table.ForeignKey(
                        name: "FK_nexusmods_user_to_integration_steam_nexusmods_user_nexusmod~",
                        column: x => x.nexusmods_user_to_steam_id,
                        principalSchema: "nexusmods_user",
                        principalTable: "nexusmods_user",
                        principalColumn: "nexusmods_user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "autocomplete",
                schema: "autocomplete",
                columns: table => new
                {
                    tenant = table.Column<byte>(type: "smallint", nullable: false),
                    autocomplete_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_autocomplete", x => new { x.tenant, x.autocomplete_id });
                    table.ForeignKey(
                        name: "FK_autocomplete_tenant_tenant",
                        column: x => x.tenant,
                        principalSchema: "tenant",
                        principalTable: "tenant",
                        principalColumn: "tenant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "crash_report_file_ignored",
                schema: "crashreport",
                columns: table => new
                {
                    tenant = table.Column<byte>(type: "smallint", nullable: false),
                    crash_report_file_ignored_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crash_report_file_ignored", x => new { x.tenant, x.crash_report_file_ignored_id });
                    table.ForeignKey(
                        name: "FK_crash_report_file_ignored_tenant_tenant",
                        column: x => x.tenant,
                        principalSchema: "tenant",
                        principalTable: "tenant",
                        principalColumn: "tenant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exception_type",
                schema: "exception",
                columns: table => new
                {
                    tenant = table.Column<byte>(type: "smallint", nullable: false),
                    exception_type_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exception_type", x => new { x.tenant, x.exception_type_id });
                    table.ForeignKey(
                        name: "FK_exception_type_tenant_tenant",
                        column: x => x.tenant,
                        principalSchema: "tenant",
                        principalTable: "tenant",
                        principalColumn: "tenant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "module",
                schema: "module",
                columns: table => new
                {
                    tenant = table.Column<byte>(type: "smallint", nullable: false),
                    module_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_module", x => new { x.tenant, x.module_id });
                    table.ForeignKey(
                        name: "FK_module_tenant_tenant",
                        column: x => x.tenant,
                        principalSchema: "tenant",
                        principalTable: "tenant",
                        principalColumn: "tenant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nexusmods_article_entity",
                schema: "nexusmods_article",
                columns: table => new
                {
                    tenant = table.Column<byte>(type: "smallint", nullable: false),
                    nexusmods_article_entity_id = table.Column<short>(type: "smallint", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    author_id = table.Column<int>(type: "integer", nullable: false),
                    create_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexusmods_article_entity", x => new { x.tenant, x.nexusmods_article_entity_id });
                    table.ForeignKey(
                        name: "FK_nexusmods_article_entity_nexusmods_user_author_id",
                        column: x => x.author_id,
                        principalSchema: "nexusmods_user",
                        principalTable: "nexusmods_user",
                        principalColumn: "nexusmods_user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nexusmods_article_entity_tenant_tenant",
                        column: x => x.tenant,
                        principalSchema: "tenant",
                        principalTable: "tenant",
                        principalColumn: "tenant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nexusmods_mod",
                schema: "nexusmods_mod",
                columns: table => new
                {
                    tenant = table.Column<byte>(type: "smallint", nullable: false),
                    nexusmods_mod_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexusmods_mod", x => new { x.tenant, x.nexusmods_mod_id });
                    table.ForeignKey(
                        name: "FK_nexusmods_mod_tenant_tenant",
                        column: x => x.tenant,
                        principalSchema: "tenant",
                        principalTable: "tenant",
                        principalColumn: "tenant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "integration_discord_tokens",
                schema: "integration",
                columns: table => new
                {
                    integration_discord_tokens_id = table.Column<int>(type: "integer", nullable: false),
                    discord_user_id = table.Column<string>(type: "text", nullable: false),
                    refresh_token = table.Column<string>(type: "text", nullable: false),
                    access_token = table.Column<string>(type: "text", nullable: false),
                    access_token_expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_integration_discord_tokens", x => x.integration_discord_tokens_id);
                    table.ForeignKey(
                        name: "FK_integration_discord_tokens_nexusmods_user_integration_disco~",
                        column: x => x.integration_discord_tokens_id,
                        principalSchema: "nexusmods_user",
                        principalTable: "nexusmods_user",
                        principalColumn: "nexusmods_user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_integration_discord_tokens_nexusmods_user_to_integration_di~",
                        column: x => x.discord_user_id,
                        principalSchema: "nexusmods_user",
                        principalTable: "nexusmods_user_to_integration_discord",
                        principalColumn: "discord_user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "integration_gog_owned_tenant",
                schema: "integration",
                columns: table => new
                {
                    integration_gog_owned_tenant_id = table.Column<string>(type: "text", nullable: false),
                    owned_tenant = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_integration_gog_owned_tenant", x => new { x.integration_gog_owned_tenant_id, x.owned_tenant });
                    table.ForeignKey(
                        name: "FK_integration_gog_owned_tenant_nexusmods_user_to_integration_~",
                        column: x => x.integration_gog_owned_tenant_id,
                        principalSchema: "nexusmods_user",
                        principalTable: "nexusmods_user_to_integration_gog",
                        principalColumn: "gog_user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_integration_gog_owned_tenant_tenant_owned_tenant",
                        column: x => x.owned_tenant,
                        principalSchema: "tenant",
                        principalTable: "tenant",
                        principalColumn: "tenant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "integration_gog_tokens",
                schema: "integration",
                columns: table => new
                {
                    integration_gog_tokens_id = table.Column<int>(type: "integer", nullable: false),
                    gog_user_id = table.Column<string>(type: "text", nullable: false),
                    refresh_token = table.Column<string>(type: "text", nullable: false),
                    access_token = table.Column<string>(type: "text", nullable: false),
                    access_token_expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_integration_gog_tokens", x => x.integration_gog_tokens_id);
                    table.ForeignKey(
                        name: "FK_integration_gog_tokens_nexusmods_user_integration_gog_token~",
                        column: x => x.integration_gog_tokens_id,
                        principalSchema: "nexusmods_user",
                        principalTable: "nexusmods_user",
                        principalColumn: "nexusmods_user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_integration_gog_tokens_nexusmods_user_to_integration_gog_go~",
                        column: x => x.gog_user_id,
                        principalSchema: "nexusmods_user",
                        principalTable: "nexusmods_user_to_integration_gog",
                        principalColumn: "gog_user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "integration_steam_owned_tenant",
                schema: "integration",
                columns: table => new
                {
                    integration_steam_owned_tenant_id = table.Column<string>(type: "text", nullable: false),
                    owned_tenant = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_integration_steam_owned_tenant", x => new { x.integration_steam_owned_tenant_id, x.owned_tenant });
                    table.ForeignKey(
                        name: "FK_integration_steam_owned_tenant_nexusmods_user_to_integratio~",
                        column: x => x.integration_steam_owned_tenant_id,
                        principalSchema: "nexusmods_user",
                        principalTable: "nexusmods_user_to_integration_steam",
                        principalColumn: "steam_user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_integration_steam_owned_tenant_tenant_owned_tenant",
                        column: x => x.owned_tenant,
                        principalSchema: "tenant",
                        principalTable: "tenant",
                        principalColumn: "tenant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "integration_steam_tokens",
                schema: "integration",
                columns: table => new
                {
                    integration_steam_tokens_id = table.Column<int>(type: "integer", nullable: false),
                    steam_user_id = table.Column<string>(type: "text", nullable: false),
                    data = table.Column<Dictionary<string, string>>(type: "hstore", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_integration_steam_tokens", x => x.integration_steam_tokens_id);
                    table.ForeignKey(
                        name: "FK_integration_steam_tokens_nexusmods_user_integration_steam_t~",
                        column: x => x.integration_steam_tokens_id,
                        principalSchema: "nexusmods_user",
                        principalTable: "nexusmods_user",
                        principalColumn: "nexusmods_user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_integration_steam_tokens_nexusmods_user_to_integration_stea~",
                        column: x => x.steam_user_id,
                        principalSchema: "nexusmods_user",
                        principalTable: "nexusmods_user_to_integration_steam",
                        principalColumn: "steam_user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "crash_report",
                schema: "crashreport",
                columns: table => new
                {
                    tenant = table.Column<byte>(type: "smallint", nullable: false),
                    crash_report_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version = table.Column<byte>(type: "smallint", nullable: false),
                    game_version = table.Column<string>(type: "text", nullable: false),
                    exception_type_id = table.Column<string>(type: "text", nullable: true),
                    exception = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crash_report", x => new { x.tenant, x.crash_report_id });
                    table.ForeignKey(
                        name: "FK_crash_report_exception_type_tenant_exception_type_id",
                        columns: x => new { x.tenant, x.exception_type_id },
                        principalSchema: "exception",
                        principalTable: "exception_type",
                        principalColumns: new[] { "tenant", "exception_type_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_crash_report_tenant_tenant",
                        column: x => x.tenant,
                        principalSchema: "tenant",
                        principalTable: "tenant",
                        principalColumn: "tenant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "top_exceptions_type",
                schema: "statistics",
                columns: table => new
                {
                    tenant = table.Column<byte>(type: "smallint", nullable: false),
                    top_exceptions_type_id = table.Column<string>(type: "text", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_top_exceptions_type", x => new { x.tenant, x.top_exceptions_type_id });
                    table.ForeignKey(
                        name: "FK_top_exceptions_type_exception_type_tenant_top_exceptions_ty~",
                        columns: x => new { x.tenant, x.top_exceptions_type_id },
                        principalSchema: "exception",
                        principalTable: "exception_type",
                        principalColumns: new[] { "tenant", "exception_type_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_top_exceptions_type_tenant_tenant",
                        column: x => x.tenant,
                        principalSchema: "tenant",
                        principalTable: "tenant",
                        principalColumn: "tenant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "crash_score_involved",
                schema: "statistics",
                columns: table => new
                {
                    tenant = table.Column<byte>(type: "smallint", nullable: false),
                    crash_score_involved_id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_version = table.Column<string>(type: "text", nullable: false),
                    module_id = table.Column<string>(type: "text", nullable: false),
                    module_version = table.Column<string>(type: "text", nullable: false),
                    involved_count = table.Column<int>(type: "integer", nullable: false),
                    not_involved_count = table.Column<int>(type: "integer", nullable: false),
                    total_count = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<int>(type: "integer", nullable: false),
                    crash_score = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crash_score_involved", x => new { x.tenant, x.crash_score_involved_id });
                    table.ForeignKey(
                        name: "FK_crash_score_involved_module_tenant_module_id",
                        columns: x => new { x.tenant, x.module_id },
                        principalSchema: "module",
                        principalTable: "module",
                        principalColumns: new[] { "tenant", "module_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_crash_score_involved_tenant_tenant",
                        column: x => x.tenant,
                        principalSchema: "tenant",
                        principalTable: "tenant",
                        principalColumn: "tenant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nexusmods_user_module",
                schema: "nexusmods_user",
                columns: table => new
                {
                    tenant = table.Column<byte>(type: "smallint", nullable: false),
                    nexusmods_user_module_link_type_id = table.Column<int>(type: "integer", nullable: false),
                    nexusmods_user_module_id = table.Column<int>(type: "integer", nullable: false),
                    module_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexusmods_user_module", x => new { x.tenant, x.nexusmods_user_module_id, x.module_id, x.nexusmods_user_module_link_type_id });
                    table.ForeignKey(
                        name: "FK_nexusmods_user_module_module_tenant_module_id",
                        columns: x => new { x.tenant, x.module_id },
                        principalSchema: "module",
                        principalTable: "module",
                        principalColumns: new[] { "tenant", "module_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nexusmods_user_module_nexusmods_user_nexusmods_user_module_~",
                        column: x => x.nexusmods_user_module_id,
                        principalSchema: "nexusmods_user",
                        principalTable: "nexusmods_user",
                        principalColumn: "nexusmods_user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nexusmods_user_module_tenant_tenant",
                        column: x => x.tenant,
                        principalSchema: "tenant",
                        principalTable: "tenant",
                        principalColumn: "tenant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nexusmods_mod_file_update",
                schema: "nexusmods_mod",
                columns: table => new
                {
                    tenant = table.Column<byte>(type: "smallint", nullable: false),
                    nexusmods_mod_file_update_id = table.Column<int>(type: "integer", nullable: false),
                    date_of_last_check = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexusmods_mod_file_update", x => new { x.tenant, x.nexusmods_mod_file_update_id });
                    table.ForeignKey(
                        name: "FK_nexusmods_mod_file_update_nexusmods_mod_tenant_nexusmods_mo~",
                        columns: x => new { x.tenant, x.nexusmods_mod_file_update_id },
                        principalSchema: "nexusmods_mod",
                        principalTable: "nexusmods_mod",
                        principalColumns: new[] { "tenant", "nexusmods_mod_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nexusmods_mod_file_update_tenant_tenant",
                        column: x => x.tenant,
                        principalSchema: "tenant",
                        principalTable: "tenant",
                        principalColumn: "tenant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nexusmods_mod_module",
                schema: "nexusmods_mod",
                columns: table => new
                {
                    tenant = table.Column<byte>(type: "smallint", nullable: false),
                    nexusmods_mod_module_link_type_id = table.Column<int>(type: "integer", nullable: false),
                    nexusmods_mod_module_id = table.Column<int>(type: "integer", nullable: false),
                    module_id = table.Column<string>(type: "text", nullable: false),
                    date_of_last_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexusmods_mod_module", x => new { x.tenant, x.nexusmods_mod_module_id, x.module_id, x.nexusmods_mod_module_link_type_id });
                    table.ForeignKey(
                        name: "FK_nexusmods_mod_module_module_tenant_module_id",
                        columns: x => new { x.tenant, x.module_id },
                        principalSchema: "module",
                        principalTable: "module",
                        principalColumns: new[] { "tenant", "module_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nexusmods_mod_module_nexusmods_mod_tenant_nexusmods_mod_mod~",
                        columns: x => new { x.tenant, x.nexusmods_mod_module_id },
                        principalSchema: "nexusmods_mod",
                        principalTable: "nexusmods_mod",
                        principalColumns: new[] { "tenant", "nexusmods_mod_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nexusmods_mod_module_tenant_tenant",
                        column: x => x.tenant,
                        principalSchema: "tenant",
                        principalTable: "tenant",
                        principalColumn: "tenant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nexusmods_mod_name",
                schema: "nexusmods_mod",
                columns: table => new
                {
                    tenant = table.Column<byte>(type: "smallint", nullable: false),
                    nexusmods_mod_name_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexusmods_mod_name", x => new { x.tenant, x.nexusmods_mod_name_id });
                    table.ForeignKey(
                        name: "FK_nexusmods_mod_name_nexusmods_mod_tenant_nexusmods_mod_name_~",
                        columns: x => new { x.tenant, x.nexusmods_mod_name_id },
                        principalSchema: "nexusmods_mod",
                        principalTable: "nexusmods_mod",
                        principalColumns: new[] { "tenant", "nexusmods_mod_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nexusmods_mod_name_tenant_tenant",
                        column: x => x.tenant,
                        principalSchema: "tenant",
                        principalTable: "tenant",
                        principalColumn: "tenant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nexusmods_user_nexusmods_mod",
                schema: "nexusmods_user",
                columns: table => new
                {
                    tenant = table.Column<byte>(type: "smallint", nullable: false),
                    nexusmods_user_nexusmods_mod_link_type_id = table.Column<int>(type: "integer", nullable: false),
                    nexusmods_user_nexusmods_mod_id = table.Column<int>(type: "integer", nullable: false),
                    nexusmods_mod_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexusmods_user_nexusmods_mod", x => new { x.tenant, x.nexusmods_user_nexusmods_mod_id, x.nexusmods_mod_id, x.nexusmods_user_nexusmods_mod_link_type_id });
                    table.ForeignKey(
                        name: "FK_nexusmods_user_nexusmods_mod_nexusmods_mod_tenant_nexusmods~",
                        columns: x => new { x.tenant, x.nexusmods_mod_id },
                        principalSchema: "nexusmods_mod",
                        principalTable: "nexusmods_mod",
                        principalColumns: new[] { "tenant", "nexusmods_mod_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nexusmods_user_nexusmods_mod_nexusmods_user_nexusmods_user_~",
                        column: x => x.nexusmods_user_nexusmods_mod_id,
                        principalSchema: "nexusmods_user",
                        principalTable: "nexusmods_user",
                        principalColumn: "nexusmods_user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nexusmods_user_nexusmods_mod_tenant_tenant",
                        column: x => x.tenant,
                        principalSchema: "tenant",
                        principalTable: "tenant",
                        principalColumn: "tenant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "crash_report_file",
                schema: "crashreport",
                columns: table => new
                {
                    tenant = table.Column<byte>(type: "smallint", nullable: false),
                    crash_report_file_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crash_report_file", x => new { x.tenant, x.crash_report_file_id });
                    table.ForeignKey(
                        name: "FK_crash_report_file_crash_report_tenant_crash_report_file_id",
                        columns: x => new { x.tenant, x.crash_report_file_id },
                        principalSchema: "crashreport",
                        principalTable: "crash_report",
                        principalColumns: new[] { "tenant", "crash_report_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_crash_report_file_tenant_tenant",
                        column: x => x.tenant,
                        principalSchema: "tenant",
                        principalTable: "tenant",
                        principalColumn: "tenant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "crash_report_metadata",
                schema: "crashreport",
                columns: table => new
                {
                    tenant = table.Column<byte>(type: "smallint", nullable: false),
                    crash_report_metadata_id = table.Column<Guid>(type: "uuid", nullable: false),
                    launcher_type = table.Column<string>(type: "text", nullable: true),
                    launcher_version = table.Column<string>(type: "text", nullable: true),
                    runtime = table.Column<string>(type: "text", nullable: true),
                    butrloader_version = table.Column<string>(type: "text", nullable: true),
                    blse_version = table.Column<string>(type: "text", nullable: true),
                    launcherex_version = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crash_report_metadata", x => new { x.tenant, x.crash_report_metadata_id });
                    table.ForeignKey(
                        name: "FK_crash_report_metadata_crash_report_tenant_crash_report_meta~",
                        columns: x => new { x.tenant, x.crash_report_metadata_id },
                        principalSchema: "crashreport",
                        principalTable: "crash_report",
                        principalColumns: new[] { "tenant", "crash_report_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_crash_report_metadata_tenant_tenant",
                        column: x => x.tenant,
                        principalSchema: "tenant",
                        principalTable: "tenant",
                        principalColumn: "tenant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "crash_report_module_info",
                schema: "crashreport",
                columns: table => new
                {
                    tenant = table.Column<byte>(type: "smallint", nullable: false),
                    crash_report_module_info_id = table.Column<Guid>(type: "uuid", nullable: false),
                    module_id = table.Column<string>(type: "text", nullable: false),
                    version = table.Column<string>(type: "text", nullable: false),
                    nexusmods_mod_id = table.Column<int>(type: "integer", nullable: true),
                    is_involved = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crash_report_module_info", x => new { x.tenant, x.crash_report_module_info_id, x.module_id });
                    table.ForeignKey(
                        name: "FK_crash_report_module_info_crash_report_tenant_crash_report_m~",
                        columns: x => new { x.tenant, x.crash_report_module_info_id },
                        principalSchema: "crashreport",
                        principalTable: "crash_report",
                        principalColumns: new[] { "tenant", "crash_report_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_crash_report_module_info_module_tenant_module_id",
                        columns: x => new { x.tenant, x.module_id },
                        principalSchema: "module",
                        principalTable: "module",
                        principalColumns: new[] { "tenant", "module_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_crash_report_module_info_nexusmods_mod_tenant_nexusmods_mod~",
                        columns: x => new { x.tenant, x.nexusmods_mod_id },
                        principalSchema: "nexusmods_mod",
                        principalTable: "nexusmods_mod",
                        principalColumns: new[] { "tenant", "nexusmods_mod_id" },
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_crash_report_module_info_tenant_tenant",
                        column: x => x.tenant,
                        principalSchema: "tenant",
                        principalTable: "tenant",
                        principalColumn: "tenant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nexusmods_user_crash_report",
                schema: "nexusmods_user",
                columns: table => new
                {
                    tenant = table.Column<byte>(type: "smallint", nullable: false),
                    crash_report_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nexusmods_user_crash_report_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexusmods_user_crash_report", x => new { x.tenant, x.nexusmods_user_crash_report_id, x.crash_report_id });
                    table.ForeignKey(
                        name: "FK_nexusmods_user_crash_report_crash_report_tenant_crash_repor~",
                        columns: x => new { x.tenant, x.crash_report_id },
                        principalSchema: "crashreport",
                        principalTable: "crash_report",
                        principalColumns: new[] { "tenant", "crash_report_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nexusmods_user_crash_report_nexusmods_user_nexusmods_user_c~",
                        column: x => x.nexusmods_user_crash_report_id,
                        principalSchema: "nexusmods_user",
                        principalTable: "nexusmods_user",
                        principalColumn: "nexusmods_user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nexusmods_user_crash_report_tenant_tenant",
                        column: x => x.tenant,
                        principalSchema: "tenant",
                        principalTable: "tenant",
                        principalColumn: "tenant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_autocomplete_type",
                schema: "autocomplete",
                table: "autocomplete",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_crash_report_tenant_exception_type_id",
                schema: "crashreport",
                table: "crash_report",
                columns: new[] { "tenant", "exception_type_id" });

            migrationBuilder.CreateIndex(
                name: "IX_crash_report_module_info_tenant_module_id",
                schema: "crashreport",
                table: "crash_report_module_info",
                columns: new[] { "tenant", "module_id" });

            migrationBuilder.CreateIndex(
                name: "IX_crash_report_module_info_tenant_nexusmods_mod_id",
                schema: "crashreport",
                table: "crash_report_module_info",
                columns: new[] { "tenant", "nexusmods_mod_id" });

            migrationBuilder.CreateIndex(
                name: "IX_crash_score_involved_tenant_module_id",
                schema: "statistics",
                table: "crash_score_involved",
                columns: new[] { "tenant", "module_id" });

            migrationBuilder.CreateIndex(
                name: "IX_integration_discord_tokens_discord_user_id",
                schema: "integration",
                table: "integration_discord_tokens",
                column: "discord_user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_integration_gog_owned_tenant_owned_tenant",
                schema: "integration",
                table: "integration_gog_owned_tenant",
                column: "owned_tenant");

            migrationBuilder.CreateIndex(
                name: "IX_integration_gog_tokens_gog_user_id",
                schema: "integration",
                table: "integration_gog_tokens",
                column: "gog_user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_integration_steam_owned_tenant_owned_tenant",
                schema: "integration",
                table: "integration_steam_owned_tenant",
                column: "owned_tenant");

            migrationBuilder.CreateIndex(
                name: "IX_integration_steam_tokens_steam_user_id",
                schema: "integration",
                table: "integration_steam_tokens",
                column: "steam_user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_nexusmods_article_entity_author_id",
                schema: "nexusmods_article",
                table: "nexusmods_article_entity",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "IX_nexusmods_mod_module_tenant_module_id",
                schema: "nexusmods_mod",
                table: "nexusmods_mod_module",
                columns: new[] { "tenant", "module_id" });

            migrationBuilder.CreateIndex(
                name: "IX_nexusmods_user_crash_report_nexusmods_user_crash_report_id",
                schema: "nexusmods_user",
                table: "nexusmods_user_crash_report",
                column: "nexusmods_user_crash_report_id");

            migrationBuilder.CreateIndex(
                name: "IX_nexusmods_user_crash_report_tenant_crash_report_id",
                schema: "nexusmods_user",
                table: "nexusmods_user_crash_report",
                columns: new[] { "tenant", "crash_report_id" });

            migrationBuilder.CreateIndex(
                name: "IX_nexusmods_user_module_nexusmods_user_module_id",
                schema: "nexusmods_user",
                table: "nexusmods_user_module",
                column: "nexusmods_user_module_id");

            migrationBuilder.CreateIndex(
                name: "IX_nexusmods_user_module_tenant_module_id",
                schema: "nexusmods_user",
                table: "nexusmods_user_module",
                columns: new[] { "tenant", "module_id" });

            migrationBuilder.CreateIndex(
                name: "IX_nexusmods_user_nexusmods_mod_nexusmods_user_nexusmods_mod_id",
                schema: "nexusmods_user",
                table: "nexusmods_user_nexusmods_mod",
                column: "nexusmods_user_nexusmods_mod_id");

            migrationBuilder.CreateIndex(
                name: "IX_nexusmods_user_nexusmods_mod_tenant_nexusmods_mod_id",
                schema: "nexusmods_user",
                table: "nexusmods_user_nexusmods_mod",
                columns: new[] { "tenant", "nexusmods_mod_id" });

            migrationBuilder.CreateIndex(
                name: "IX_nexusmods_user_role_nexusmods_user_role_id",
                schema: "nexusmods_user",
                table: "nexusmods_user_role",
                column: "nexusmods_user_role_id");

            migrationBuilder.CreateIndex(
                name: "IX_quartz_execution_log_date_added_utc_log_type",
                schema: "quartz",
                table: "quartz_execution_log",
                columns: new[] { "date_added_utc", "log_type" });

            migrationBuilder.CreateIndex(
                name: "IX_quartz_execution_log_run_instance_id",
                schema: "quartz",
                table: "quartz_execution_log",
                column: "run_instance_id");

            migrationBuilder.CreateIndex(
                name: "IX_quartz_execution_log_trigger_name_trigger_group_job_name_jo~",
                schema: "quartz",
                table: "quartz_execution_log",
                columns: new[] { "trigger_name", "trigger_group", "job_name", "job_group", "date_added_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "autocomplete",
                schema: "autocomplete");

            migrationBuilder.DropTable(
                name: "crash_report_file",
                schema: "crashreport");

            migrationBuilder.DropTable(
                name: "crash_report_file_ignored",
                schema: "crashreport");

            migrationBuilder.DropTable(
                name: "crash_report_metadata",
                schema: "crashreport");

            migrationBuilder.DropTable(
                name: "crash_report_module_info",
                schema: "crashreport");

            migrationBuilder.DropTable(
                name: "crash_score_involved",
                schema: "statistics");

            migrationBuilder.DropTable(
                name: "integration_discord_tokens",
                schema: "integration");

            migrationBuilder.DropTable(
                name: "integration_gog_owned_tenant",
                schema: "integration");

            migrationBuilder.DropTable(
                name: "integration_gog_tokens",
                schema: "integration");

            migrationBuilder.DropTable(
                name: "integration_steam_owned_tenant",
                schema: "integration");

            migrationBuilder.DropTable(
                name: "integration_steam_tokens",
                schema: "integration");

            migrationBuilder.DropTable(
                name: "nexusmods_article_entity",
                schema: "nexusmods_article");

            migrationBuilder.DropTable(
                name: "nexusmods_mod_file_update",
                schema: "nexusmods_mod");

            migrationBuilder.DropTable(
                name: "nexusmods_mod_module",
                schema: "nexusmods_mod");

            migrationBuilder.DropTable(
                name: "nexusmods_mod_name",
                schema: "nexusmods_mod");

            migrationBuilder.DropTable(
                name: "nexusmods_user_crash_report",
                schema: "nexusmods_user");

            migrationBuilder.DropTable(
                name: "nexusmods_user_module",
                schema: "nexusmods_user");

            migrationBuilder.DropTable(
                name: "nexusmods_user_name",
                schema: "nexusmods_user");

            migrationBuilder.DropTable(
                name: "nexusmods_user_nexusmods_mod",
                schema: "nexusmods_user");

            migrationBuilder.DropTable(
                name: "nexusmods_user_role",
                schema: "nexusmods_user");

            migrationBuilder.DropTable(
                name: "quartz_execution_log",
                schema: "quartz");

            migrationBuilder.DropTable(
                name: "top_exceptions_type",
                schema: "statistics");

            migrationBuilder.DropTable(
                name: "nexusmods_user_to_integration_discord",
                schema: "nexusmods_user");

            migrationBuilder.DropTable(
                name: "nexusmods_user_to_integration_gog",
                schema: "nexusmods_user");

            migrationBuilder.DropTable(
                name: "nexusmods_user_to_integration_steam",
                schema: "nexusmods_user");

            migrationBuilder.DropTable(
                name: "crash_report",
                schema: "crashreport");

            migrationBuilder.DropTable(
                name: "module",
                schema: "module");

            migrationBuilder.DropTable(
                name: "nexusmods_mod",
                schema: "nexusmods_mod");

            migrationBuilder.DropTable(
                name: "nexusmods_user",
                schema: "nexusmods_user");

            migrationBuilder.DropTable(
                name: "exception_type",
                schema: "exception");

            migrationBuilder.DropTable(
                name: "tenant",
                schema: "tenant");
        }
    }
}
