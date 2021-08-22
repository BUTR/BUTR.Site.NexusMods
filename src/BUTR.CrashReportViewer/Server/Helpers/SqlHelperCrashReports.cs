using BUTR.CrashReportViewer.Server.Extensions;
using BUTR.CrashReportViewer.Server.Models.Database;

using Microsoft.Extensions.Configuration;

using Npgsql;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.CrashReportViewer.Server.Helpers
{
    public class SqlHelperCrashReports
    {
        /// <summary>
        /// @crashReportId
        /// </summary>
        public static string CrashReportGet = @"
SELECT
    *
FROM
    crash_report_entity
WHERE
    id = @crashReportId
;";

        /// <summary>
        /// @id, @exception, @createdAt, @modIds, @url
        /// </summary>
        public static string CrashReportUpsert = @"
INSERT INTO crash_report_entity(id, exception, created_at, mod_ids, url)
VALUES (@id, @exception, @createdAt, @modIds, @url)
ON CONFLICT ON CONSTRAINT crash_report_entity_pkey
DO UPDATE SET exception=@exception, modIds=@modIds, url=@url
RETURNING *
;";

        /// <summary>
        /// @val
        /// </summary>
        public static string CrashReportGetCountByUser = @"
SELECT
    COUNT(*)
FROM
    crash_report_entity
WHERE
    (@userId != -1 AND (SELECT ARRAY_AGG(mod_id) as mod_ids FROM mod_entity WHERE @userId = ANY(user_ids)) && mod_ids) OR (@userId = -1)
;";

        /// <summary>
        /// @val, @offset, @limit
        /// </summary>
        public static string CrashReportGetPaginatedByUser = @"
SELECT
    *
FROM
    crash_report_entity
WHERE
    (@userId != -1 AND (SELECT ARRAY_AGG(mod_id) as mod_ids FROM mod_entity WHERE @userId = ANY(user_ids)) && mod_ids) OR (@userId = -1)
ORDER BY
    created_at
OFFSET
    @offset
LIMIT
    @limit
;";


        private readonly IConfiguration _configuration;

        public SqlHelperCrashReports(IConfiguration configuration) => _configuration = configuration;

        public async Task<CrashReportTableEntry?> FindAsync(Guid crashReportId, CancellationToken ct = default)
        {
            await using var connection = new NpgsqlConnection(_configuration.GetConnectionString("Main"));
            await connection.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(CrashReportGet, connection);
            cmd.Parameters.Add(new NpgsqlParameter<Guid>("crashReportId", crashReportId));
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            return await reader.ReadAsync(ct) ? await CreateCrashReportTableEntryFromReaderAsync(reader, ct) : null;
        }

        public async Task<CrashReportTableEntry?> UpsertAsync(CrashReportTableEntry crashReport, CancellationToken ct = default)
        {
            await using var connection = new NpgsqlConnection(_configuration.GetConnectionString("Main"));
            await connection.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(CrashReportUpsert, connection);
            cmd.Parameters.Add(new NpgsqlParameter<Guid>("id", crashReport.Id));
            cmd.Parameters.Add(new NpgsqlParameter<string>("exception", crashReport.Exception));
            cmd.Parameters.Add(new NpgsqlParameter<DateTime>("createdAt", crashReport.CreatedAt));
            cmd.Parameters.Add(new NpgsqlParameter<int[]>("modIds", crashReport.ModIds));
            cmd.Parameters.Add(new NpgsqlParameter<string>("url", crashReport.Url));
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            return await reader.ReadAsync(ct) ? await CreateCrashReportTableEntryFromReaderAsync(reader, ct) : null;
        }

        public async Task<(int, List<CrashReportTableEntry>)> GetAsync(int userId, int skip, int take, CancellationToken ct = default)
        {
            static async Task<int> GetCount(NpgsqlDataReader reader, CancellationToken ct = default)
            {
                return await reader.ReadAsync(ct) ? reader.GetInt32(0) : 0;
            }
            static async IAsyncEnumerable<CrashReportTableEntry> GetAllCrashReports(NpgsqlDataReader reader, [EnumeratorCancellation] CancellationToken ct = default)
            {
                while (await reader.ReadAsync(ct))
                {
                    yield return await CreateCrashReportTableEntryFromReaderAsync(reader, ct);
                }
            }

            await using var connection = new NpgsqlConnection(_configuration.GetConnectionString("Main"));
            await connection.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(CrashReportGetCountByUser + CrashReportGetPaginatedByUser, connection);
            cmd.Parameters.Add(new NpgsqlParameter<int>("userId", userId));
            cmd.Parameters.Add(new NpgsqlParameter<int>("offset", skip));
            cmd.Parameters.Add(new NpgsqlParameter<int>("limit", take));
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            var count = await GetCount(reader, ct);
            await reader.NextResultAsync(ct);
            var result = await GetAllCrashReports(reader, ct).ToListAsync(ct);
            return (count, result);
        }

        public static async Task<CrashReportTableEntry> CreateCrashReportTableEntryFromReaderAsync(NpgsqlDataReader reader, CancellationToken ct = default)
        {
            var id = await reader.GetNullableGuidAsync("id", ct);
            var exception = await reader.GetNullableStringAsync("exception", ct);
            var createdAt = await reader.GetNullableTimeStampAsync("created_at", ct);
            var modIds = await reader.GetNullableInt32ArrayAsync("mod_ids", ct);
            var url = await reader.GetNullableStringAsync("url", ct);

            return new CrashReportTableEntry
            {
                Id = id ?? Guid.Empty,
                Exception = exception ?? string.Empty,
                CreatedAt = createdAt ?? DateTime.MinValue,
                ModIds = modIds ?? Array.Empty<int>(),
                Url = url ?? string.Empty,
                UserCrashReports = new List<UserCrashReportTableEntry>()
            };
        }
    }
}