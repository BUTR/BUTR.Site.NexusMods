using BUTR.Site.NexusMods.Server.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Npgsql;

using Polly;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services.Database
{
    public class SeederProvider
    {
        public static string CreateModsTable = @"
CREATE TABLE IF NOT EXISTS ""public"".""mod_entity"" (
    ""game_domain"" text NOT NULL,
    ""mod_id"" int4 NOT NULL,
    ""name"" text NOT NULL,
    ""user_ids"" _int4 NOT NULL,
    CONSTRAINT mod_entity_pkey PRIMARY KEY (""game_domain"",""mod_id"")
)
;";

        public static string CreateCrashReportTable = @"
CREATE TABLE IF NOT EXISTS ""public"".""crash_report_entity"" (
    ""id"" uuid NOT NULL,
    ""exception"" text NOT NULL,
    ""created_at"" timestamp NOT NULL,
    ""mod_ids"" _int4 NOT NULL,
    ""url"" text NOT NULL,
    CONSTRAINT crash_report_entity_pkey PRIMARY KEY (""id"")
)
;";

        public static string CreateUserCrashReportTable = @"
CREATE TABLE IF NOT EXISTS ""public"".""user_crash_report_entity"" (
    ""user_id"" int4 NOT NULL,
    ""crash_report_id"" uuid NOT NULL,
    ""status"" int4 NOT NULL,
    ""comment"" text NOT NULL,
    CONSTRAINT ""FK_user_crash_report_entity_crash_report_entity_crash_report_id"" FOREIGN KEY (""crash_report_id"") REFERENCES ""public"".""crash_report_entity""(""id"") ON DELETE CASCADE,
    CONSTRAINT user_crash_report_entity_pkey PRIMARY KEY (""user_id"",""crash_report_id"")
)
;";

        public static string CreateModListTable = @"
CREATE TABLE IF NOT EXISTS ""public"".""modlist_entity"" (
    ""id"" uuid NOT NULL,
    ""name"" text NOT NULL,
    ""user_id"" int4 NOT NULL,
    ""content"" text NOT NULL,
    CONSTRAINT modlist_entity_pkey PRIMARY KEY (""id"")
)
;";

        public static string CreateRoleTable = @"
CREATE TABLE IF NOT EXISTS ""public"".""role_entity"" (
    ""user_id"" int4 NOT NULL,
    ""role"" text NOT NULL,
    CONSTRAINT role_entity_pkey PRIMARY KEY (""user_id"")
)
;";


        private readonly ILogger _logger;
        private readonly ConnectionStringsOptions _options;

        public SeederProvider(ILogger<SeederProvider> logger, IOptions<ConnectionStringsOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options.Value ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task CreateTablesIfNotExistAsync(CancellationToken ct)
        {
            var policy = Policy.Handle<Exception>(ex => ex.GetType() != typeof(TaskCanceledException) || ex.GetType() != typeof(OperationCanceledException))
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(5000), (ex, time) =>
                {
                    _logger.LogError(ex, "Exception during sql init. Retrying after {RetrySeconds} seconds", time.TotalSeconds);
                });

            await policy.ExecuteAsync(async token =>
            {
                await using var connection = new NpgsqlConnection(_options.Main);
                await connection.OpenAsync(ct);
                await using var cmd = new NpgsqlCommand(
                    CreateModsTable +
                    CreateCrashReportTable +
                    CreateUserCrashReportTable +
                    CreateModListTable +
                    CreateRoleTable, connection);
                await using var reader = await cmd.ExecuteReaderAsync(ct);
                while (await reader.NextResultAsync(ct)) { }
            }, ct);
        }
    }
}