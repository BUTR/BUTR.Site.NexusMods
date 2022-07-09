using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.Extensions.Logging;

using Npgsql;

using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services.Database
{
    public sealed class UserCrashReportsProvider
    {
        /// <summary>
        /// @crashReportId, @userId
        /// </summary>
        private static string GetSql = @"
SELECT
    ucr.*,
    cr.*
FROM
    user_crash_report_entity ucr
LEFT JOIN
    crash_report_entity cr ON(ucr.crash_report_id = cr.id)
WHERE
    ucr.crash_report_id = @crashReportId AND ucr.user_id = @userId
;";

        /// <summary>
        /// @userId, @status, @comment, @crashReportId
        /// </summary>
        private static string UpsertSql = @"
WITH ucr AS (
    INSERT INTO user_crash_report_entity(user_id, status, comment, crash_report_id)
    VALUES (@userId, @status, @comment, @crashReportId)
    ON CONFLICT ON CONSTRAINT user_crash_report_entity_pkey
    DO UPDATE SET status = @status, comment = @comment
    RETURNING *)
SELECT
    ucr.*,
    cr.*
FROM
    ucr
LEFT JOIN
    crash_report_entity cr ON(ucr.crash_report_id = cr.id)
WHERE
    ucr.crash_report_id = @crashReportId AND ucr.user_id = @userId
;";


        private readonly ILogger _logger;
        private readonly MainConnectionProvider _connectionProvider;

        public UserCrashReportsProvider(ILogger<UserCrashReportsProvider> logger, MainConnectionProvider connectionProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        }

        public async Task<UserCrashReportTableEntry?> FindAsync(int userId, Guid crashReportId, CancellationToken ct = default)
        {
            await using var connection = _connectionProvider.Connection;
            await connection.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(GetSql, connection);
            cmd.Parameters.Add(new NpgsqlParameter<Guid>("crashReportId", crashReportId));
            cmd.Parameters.Add(new NpgsqlParameter<int>("userId", userId));
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                var result = await CreateUserCrashReportFromReaderAsync(reader, ct);
                result.CrashReport = await CrashReportsProvider.CreateCrashReportFromReaderAsync(reader, ct);
                return result;
            }
            
            return null;
        }

        public async Task<UserCrashReportTableEntry?> UpsertAsync(UserCrashReportTableEntry userCrashReport, CancellationToken ct = default)
        {
            await using var connection = _connectionProvider.Connection;
            await connection.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(UpsertSql, connection);
            cmd.Parameters.Add(new NpgsqlParameter<int>("userId", userCrashReport.UserId));
            cmd.Parameters.Add(new NpgsqlParameter<int>("status", (int) userCrashReport.Status));
            cmd.Parameters.Add(new NpgsqlParameter<string>("comment", userCrashReport.Comment));
            cmd.Parameters.Add(new NpgsqlParameter("crashReportId", userCrashReport.CrashReport?.Id ?? (object) DBNull.Value));
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                var result = await CreateUserCrashReportFromReaderAsync(reader, ct);
                result.CrashReport = await CrashReportsProvider.CreateCrashReportFromReaderAsync(reader, ct);
                return result;
            }
            
            return null;
        }

        private static async Task<UserCrashReportTableEntry> CreateUserCrashReportFromReaderAsync(DbDataReader reader, CancellationToken ct = default)
        {
            var userId = await reader.GetNullableInt32Async("user_id", ct);
            var status = await reader.GetNullableInt32Async("status", ct);
            var comment = await reader.GetNullableStringAsync("comment", ct);

            return new UserCrashReportTableEntry
            {
                UserId = userId ?? -1,
                Status = (CrashReportStatus) (status ?? 0),
                Comment = comment ?? string.Empty,
            };
        }
    }
}