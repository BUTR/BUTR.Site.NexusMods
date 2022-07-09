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
    public sealed class SeederProvider
    {
        private static string CreateNexusModsModTable = @"
CREATE TABLE IF NOT EXISTS public.nexusmods_mod_entity (
    game_domain text NOT NULL,
    mod_id int4 NOT NULL,
    name text NOT NULL,
    user_ids int4[] NOT NULL,
    CONSTRAINT mod_entity_pkey PRIMARY KEY (game_domain,mod_id)
)
;";

        private static string CreateCrashReportTable = @"
CREATE TABLE IF NOT EXISTS public.crash_report_entity (
    id uuid NOT NULL,
    game_version text NOT NULL,
    exception text NOT NULL,
    created_at timestamp NOT NULL,
    mod_ids text[] NOT NULL,
    involved_mod_ids text[] NOT NULL,
    mod_nexus_mods_ids int4[] NOT NULL,
    url text NOT NULL,
    CONSTRAINT crash_report_entity_pkey PRIMARY KEY (id)
)
;";

        private static string CreateUserCrashReportTable = @"
CREATE TABLE IF NOT EXISTS public.user_crash_report_entity (
    user_id int4 NOT NULL,
    crash_report_id uuid NOT NULL,
    status int4 NOT NULL,
    comment text NOT NULL,
    CONSTRAINT FK_user_crash_report_entity_crash_report_id FOREIGN KEY (crash_report_id) REFERENCES public.crash_report_entity(id) ON DELETE CASCADE,
    CONSTRAINT user_crash_report_entity_pkey PRIMARY KEY (user_id,crash_report_id)
)
;";

        private static string CreateUserModsTable = @"
CREATE TABLE IF NOT EXISTS public.user_mods2_entity (
    id uuid NOT NULL,
    name text NOT NULL,
    user_id int4 NOT NULL,
    content text NOT NULL,
    CONSTRAINT modlist_entity_pkey PRIMARY KEY (id)
)
;";

        private static string CreateUserRoleTable = @"
CREATE TABLE IF NOT EXISTS public.user_role (
    user_id int4 NOT NULL,
    role text NOT NULL,
    CONSTRAINT role_entity_pkey PRIMARY KEY (user_id)
)
;";

        private static string CreateCrashReportFileTable = @"
CREATE TABLE IF NOT EXISTS public.crash_report_file (
    filename text NOT NULL,
    crash_report_id uuid NOT NULL,
    CONSTRAINT FK_crash_report_file_entity_crash_report_id FOREIGN KEY (crash_report_id) REFERENCES public.crash_report_entity(id) ON DELETE CASCADE,
    CONSTRAINT crash_report_file_entity_pkey PRIMARY KEY (filename)
)
;";

        private static string CreateUserAllowedModsTable = @"
CREATE TABLE IF NOT EXISTS public.user_allowed_mods (
    user_id int4 NOT NULL,
    allowed_mod_ids text[] NOT NULL,
    CONSTRAINT user_mod_ids_entity_pkey PRIMARY KEY (user_id)
)
;";

        private static string CreateModNexusModsManualLinkTable = @"
CREATE TABLE IF NOT EXISTS public.mod_nexusmods_manual_link (
    mod_id text NOT NULL,
    nexus_mods_id int4 NOT NULL,
    CONSTRAINT mod_id_to_nexus_mods_id_pkey PRIMARY KEY (mod_id)
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
                    CreateNexusModsModTable +
                    CreateCrashReportTable +
                    CreateUserCrashReportTable +
                    CreateUserModsTable +
                    CreateUserRoleTable +
                    CreateCrashReportFileTable +
                    CreateUserAllowedModsTable, connection);
                await using var reader = await cmd.ExecuteReaderAsync(token);
                while (await reader.NextResultAsync(token)) { }
            }, ct);
        }
    }
}