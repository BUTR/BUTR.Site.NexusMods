using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.Extensions.Logging;

using Npgsql;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services.Database
{
    public sealed class CrashReportsProvider
    {
        /// <summary>
        /// @crashReportId
        /// </summary>
        private static string GetSql = @"
SELECT
    cr.*,
    COALESCE(NULLIF(json_agg(ucr)::JSONB, '[null]'), '[]')::TEXT as user_crash_reports
FROM
    crash_report_entity cr
LEFT JOIN
    user_crash_report_entity ucr ON(ucr.crash_report_id = cr.id)
WHERE
    id = @crashReportId
GROUP BY
    id,
    game_version,
    exception,
    created_at,
    mod_ids,
    involved_mod_ids,
    mod_nexusmods_ids,
    url
;";

        /// <summary>
        /// @id, @gameVersion, @exception, @createdAt, @modIds, @modNexusModsIds, @url
        /// </summary>
        private static string UpsertSql = @"
WITH cr AS (
    INSERT INTO crash_report_entity(id, game_version, exception, created_at, mod_ids, involved_mod_ids, mod_nexusmods_ids, url)
    VALUES (@id, @gameVersion, @exception, @createdAt, @modIds, @involvedModIds, @modNexusModsIds, @url)
    ON CONFLICT ON CONSTRAINT crash_report_entity_pkey
    DO UPDATE SET game_version = @gameVersion, exception = @exception, mod_ids = @modIds, involved_mod_ids = @involvedModIds, mod_nexusmods_ids = @modNexusModsIds, url = @url
    RETURNING *)
SELECT
    cr.*,
    COALESCE(NULLIF(json_agg(ucr)::JSONB, '[null]'), '[]')::TEXT as user_crash_reports
FROM
    cr
LEFT JOIN
    user_crash_report_entity ucr ON(ucr.crash_report_id = cr.id)
GROUP BY
    id,
    game_version,
    exception,
    created_at,
    mod_ids,
    involved_mod_ids,
    mod_nexusmods_ids,
    url
;";

        /// <summary>
        /// @userId
        /// </summary>
        private static string GetCountByUserSql = @"
SELECT
    COUNT(*)
FROM
    crash_report_entity cr
WHERE
    (
        @userId != -1
        AND
        (
            (SELECT ARRAY_AGG(mod_id) as mod_nexusmods_ids FROM nexusmods_mod_entity nm WHERE @userId = ANY(nm.user_ids)) && cr.mod_nexusmods_ids
            OR
            (SELECT allowed_mod_ids FROM user_allowed_mods umi WHERE @userId = umi.user_id) && cr.mod_ids
            OR
            (SELECT ARRAY_AGG(nexusmods_id) FROM mod_nexusmods_manual_link mnml WHERE mnml.mod_id = ANY(cr.mod_ids)) && cr.mod_nexusmods_ids
        )
    )
    OR
    (
        @userId = -1
    )
;";

        /// <summary>
        /// @val, @offset, @limit
        /// </summary>
        private static string GetPaginatedByUserSql = @"
SELECT
    cr.*,
    COALESCE(NULLIF(json_agg(ucr)::JSONB, '[null]'), '[]')::TEXT as user_crash_reports
FROM
    crash_report_entity cr
LEFT JOIN
    user_crash_report_entity ucr ON(ucr.crash_report_id = cr.id)
WHERE
    (
        @userId != -1
        AND
        (
            (SELECT ARRAY_AGG(mod_id) as mod_nexusmods_ids FROM nexusmods_mod_entity nm WHERE @userId = ANY(nm.user_ids)) && cr.mod_nexusmods_ids
            OR
            (SELECT allowed_mod_ids FROM user_allowed_mods umi WHERE @userId = umi.user_id) && cr.mod_ids
            OR
            (SELECT ARRAY_AGG(nexusmods_id) FROM mod_nexusmods_manual_link mnml WHERE mnml.mod_id = ANY(cr.mod_ids)) && cr.mod_nexusmods_ids
        )
    )
    OR
    (
        @userId = -1
    )
GROUP BY
    id,
    game_version,
    exception,
    created_at,
    mod_ids,
    involved_mod_ids,
    mod_nexusmods_ids,
    url
ORDER BY
    created_at DESC
OFFSET
    @offset
LIMIT
    @limit
;";


        private readonly ILogger _logger;
        private readonly MainConnectionProvider _connectionProvider;

        public CrashReportsProvider(ILogger<CrashReportsProvider> logger, MainConnectionProvider connectionProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        }

        public async Task<CrashReportTableEntry?> FindAsync(Guid crashReportId, CancellationToken ct = default)
        {
            await using var connection = _connectionProvider.Connection;
            await connection.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(GetSql, connection);
            cmd.Parameters.Add(new NpgsqlParameter<Guid>("crashReportId", crashReportId));
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                var result = await CreateCrashReportFromReaderAsync(reader, ct);
                result.UserCrashReports = await CreateUserCrashReportListFromReaderAsync(reader, result, ct);
                return result;
            }
            else
                return null;
        }

        public async Task<CrashReportTableEntry?> UpsertAsync(CrashReportTableEntry crashReport, CancellationToken ct = default)
        {
            await using var connection = _connectionProvider.Connection;
            await connection.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(UpsertSql, connection);
            cmd.Parameters.Add(new NpgsqlParameter<Guid>("id", crashReport.Id));
            cmd.Parameters.Add(new NpgsqlParameter<string>("gameVersion", crashReport.GameVersion));
            cmd.Parameters.Add(new NpgsqlParameter<string>("exception", crashReport.Exception));
            cmd.Parameters.Add(new NpgsqlParameter<DateTime>("createdAt", crashReport.CreatedAt));
            cmd.Parameters.Add(new NpgsqlParameter<ImmutableArray<string>>("modIds", crashReport.ModIds));
            cmd.Parameters.Add(new NpgsqlParameter<ImmutableArray<string>>("involvedModIds", crashReport.InvolvedModIds));
            cmd.Parameters.Add(new NpgsqlParameter<ImmutableArray<int>>("modNexusModsIds", crashReport.ModNexusModsIds));
            cmd.Parameters.Add(new NpgsqlParameter<string>("url", crashReport.Url));
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                var result = await CreateCrashReportFromReaderAsync(reader, ct);
                result.UserCrashReports = await CreateUserCrashReportListFromReaderAsync(reader, result, ct);
                return result;
            }
            else
                return null;
        }

        public async Task<(int, List<CrashReportTableEntry>)> GetPaginatedAsync(int userId, int skip, int take, CancellationToken ct = default)
        {
            static async Task<int> GetCount(DbDataReader reader, CancellationToken ct = default)
            {
                return await reader.ReadAsync(ct) ? reader.GetInt32(0) : 0;
            }
            static async IAsyncEnumerable<CrashReportTableEntry> GetAllCrashReports(DbDataReader reader, [EnumeratorCancellation] CancellationToken ct = default)
            {
                while (await reader.ReadAsync(ct))
                {
                    var result = await CreateCrashReportFromReaderAsync(reader, ct);
                    result.UserCrashReports = await CreateUserCrashReportListFromReaderAsync(reader, result, ct);
                    yield return result;
                }
            }

            await using var connection = _connectionProvider.Connection;
            await connection.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(GetCountByUserSql + GetPaginatedByUserSql, connection);
            cmd.Parameters.Add(new NpgsqlParameter<int>("userId", userId));
            cmd.Parameters.Add(new NpgsqlParameter<int>("offset", skip));
            cmd.Parameters.Add(new NpgsqlParameter<int>("limit", take));
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            var count = await GetCount(reader, ct);
            await reader.NextResultAsync(ct);
            var result = await GetAllCrashReports(reader, ct).ToListAsync(ct);
            return (count, result);
        }

        public static async Task<CrashReportTableEntry> CreateCrashReportFromReaderAsync(DbDataReader reader, CancellationToken ct = default)
        {
            var id = await reader.GetNullableGuidAsync("id", ct);
            var gameVersion = await reader.GetNullableStringAsync("game_version", ct);
            var exception = await reader.GetNullableStringAsync("exception", ct);
            var createdAt = await reader.GetNullableDateTimeAsync("created_at", ct);
            var modIds = await reader.GetNullableStringArrayAsync("mod_ids", ct);
            var involvedModIds = await reader.GetNullableStringArrayAsync("involved_mod_ids", ct);
            var modNexusModsIds = await reader.GetNullableInt32ArrayAsync("mod_nexusmods_ids", ct);
            var url = await reader.GetNullableStringAsync("url", ct);

            return new CrashReportTableEntry
            {
                Id = id ?? Guid.Empty,
                GameVersion = gameVersion ?? string.Empty,
                Exception = exception ?? string.Empty,
                CreatedAt = createdAt ?? DateTime.MinValue,
                ModIds = modIds ?? ImmutableArray<string>.Empty,
                InvolvedModIds = involvedModIds ?? ImmutableArray<string>.Empty,
                ModNexusModsIds = modNexusModsIds ?? ImmutableArray<int>.Empty,
                Url = url ?? string.Empty,
            };
        }
        private static async Task<ImmutableArray<UserCrashReportTableEntry>> CreateUserCrashReportListFromReaderAsync(DbDataReader reader, CrashReportTableEntry parent, CancellationToken ct = default)
        {
            var userCrashReports = await reader.GetNullableStringAsync("user_crash_reports", ct) ?? string.Empty;
            return JsonSerializer.Deserialize<List<UserCrashReportTableEntryDTO>>(userCrashReports)?
                                          .Select(x => new UserCrashReportTableEntry
                                          {
                                              UserId = x.UserId,
                                              Status = x.Status,
                                              Comment = x.Comment,
                                              CrashReport = parent
                                          }).ToImmutableArray()
                                      ?? ImmutableArray<UserCrashReportTableEntry>.Empty;
        }

        private sealed record UserCrashReportTableEntryDTO
        {
            [JsonPropertyName("user_id")]
            public int UserId { get; set; } = default!;

            [JsonIgnore]
            public CrashReportTableEntry? CrashReport { get; set; } = default!;

            [JsonPropertyName("status")]
            public CrashReportStatus Status { get; set; } = default!;

            [JsonPropertyName("comment")]
            public string Comment { get; set; } = default!;
        }
    }
}