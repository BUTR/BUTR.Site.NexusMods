using Microsoft.Extensions.Configuration;

using Npgsql;

using System.Threading;
using System.Threading.Tasks;

namespace BUTR.CrashReportViewer.Server.Helpers
{
    public class SqlHelperInit
    {
        public static string CreateModsTable = @"
CREATE TABLE IF NOT EXISTS ""public"".""mod_entity"" (
    ""game_domain"" text NOT NULL,
    ""mod_id"" int4 NOT NULL,
    ""name"" text NOT NULL,
    ""user_ids"" _int4 NOT NULL,
    PRIMARY KEY (""game_domain"",""mod_id"")
)
;";

        public static string CreateCrashReportTable = @"
CREATE TABLE IF NOT EXISTS ""public"".""crash_report_entity"" (
    ""id"" uuid NOT NULL,
    ""exception"" text NOT NULL,
    ""created_at"" timestamp NOT NULL,
    ""mod_ids"" _int4 NOT NULL,
    ""url"" text NOT NULL,
    PRIMARY KEY (""id"")
)
;";

        public static string CreateUserCrashReportTable = @"
CREATE TABLE IF NOT EXISTS ""public"".""user_crash_report_entity"" (
    ""user_id"" int4 NOT NULL,
    ""crash_report_id"" uuid NOT NULL,
    ""status"" int4 NOT NULL,
    ""comment"" text NOT NULL,
    CONSTRAINT ""FK_user_crash_report_entity_crash_report_entity_crash_report_id"" FOREIGN KEY (""crash_report_id"") REFERENCES ""public"".""crash_report_entity""(""id"") ON DELETE CASCADE,
    PRIMARY KEY (""user_id"",""crash_report_id"")
)
;";

        public static string CreateCacheTable = @"
CREATE TABLE IF NOT EXISTS ""public"".""nexusmods_cache_entry"" (
    ""Id"" text NOT NULL,
    ""AbsoluteExpiration"" timestamptz,
    ""ExpiresAtTime"" timestamptz NOT NULL,
    ""SlidingExpirationInSeconds"" int8,
    ""Value"" bytea,
    PRIMARY KEY (""Id"")
)
;";


        private readonly IConfiguration _configuration;

        public SqlHelperInit(IConfiguration configuration) => _configuration = configuration;

        public async Task CreateTablesIfNotExistAsync(CancellationToken ct)
        {
            await using var connection = new NpgsqlConnection(_configuration.GetConnectionString("Main"));
            await connection.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(
                CreateModsTable +
                CreateCrashReportTable +
                CreateUserCrashReportTable +
                CreateCacheTable, connection);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.NextResultAsync(ct)) { }
        }
    }
}