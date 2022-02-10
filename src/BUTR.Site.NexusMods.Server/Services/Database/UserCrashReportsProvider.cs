using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Shared.Models;

using Microsoft.Extensions.Logging;

using Npgsql;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services.Database
{
    public class UserCrashReportsProvider
    {
        /// <summary>
        /// @crashReportId, @userId
        /// </summary>
        public static string UserCrashReportGet = @"
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
        public static string UserCrashReportUpsert = @"
INSERT INTO user_crash_report_entity(user_id, status, comment, crash_report_id)
VALUES (@userId, @status, @comment, @crashReportId)
ON CONFLICT ON CONSTRAINT crash_report_entity_pkey
DO UPDATE SET status=@status, comment=@comment
RETURNING *
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
            await using var cmd = new NpgsqlCommand(UserCrashReportGet, connection);
            cmd.Parameters.Add(new NpgsqlParameter<Guid>("crashReportId", crashReportId));
            cmd.Parameters.Add(new NpgsqlParameter<int>("userId", userId));
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            return await reader.ReadAsync(ct) ? await CreateUserCrashReportTableEntryFromReaderAsync(reader, ct) : null;
        }

        public async Task<UserCrashReportTableEntry?> UpsertAsync(UserCrashReportTableEntry userCrashReport, CancellationToken ct = default)
        {
            await using var connection = _connectionProvider.Connection;
            await connection.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(UserCrashReportUpsert, connection);
            cmd.Parameters.Add(new NpgsqlParameter<int>("userId", userCrashReport.UserId));
            cmd.Parameters.Add(new NpgsqlParameter<int>("status", (int) userCrashReport.Status));
            cmd.Parameters.Add(new NpgsqlParameter<string>("comment", userCrashReport.Comment));
            cmd.Parameters.Add(new NpgsqlParameter<Guid?>("crashReportId", userCrashReport.CrashReport?.Id));
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            return await reader.ReadAsync(ct) ? await CreateUserCrashReportTableEntryFromReaderAsync(reader, ct) : null;
        }

        public static async Task<UserCrashReportTableEntry> CreateUserCrashReportTableEntryFromReaderAsync(NpgsqlDataReader reader, CancellationToken ct = default)
        {
            var userId = await reader.GetNullableInt32Async("user_id", ct);
            var status = await reader.GetNullableInt32Async("status", ct);
            var comment = await reader.GetNullableStringAsync("comment", ct);
            //var crashReportId = await reader.GetNullableGuidAsync("crash_report_id", ct);

            return new UserCrashReportTableEntry
            {
                UserId = userId ?? -1,
                Status = (CrashReportStatus) (status ?? 0),
                Comment = comment ?? string.Empty,
                CrashReport = await CrashReportsProvider.CreateCrashReportTableEntryFromReaderAsync(reader, ct),
            };
        }
    }
}