using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.Extensions.Logging;

using Npgsql;

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services.Database
{
    public sealed class CrashReportFileProvider
    {
        /// <summary>
        /// @filenames
        /// </summary>
        private static string FindMissingFilenamesSql = @"
SELECT
    filename
FROM
    unnest(@filenames) as filename
WHERE
    filename NOT IN (SELECT filename FROM crash_report_file)
;";

        /// <summary>
        /// @filename, @crash_report_id
        /// </summary>
        private static string UpsertSql = @"
INSERT INTO crash_report_file(filename, crash_report_id)
VALUES (@filename, @crash_report_id)
ON CONFLICT ON CONSTRAINT crash_report_file_entity_pkey
DO UPDATE SET filename = @filename
RETURNING *
;";

        private readonly ILogger _logger;
        private readonly MainConnectionProvider _connectionProvider;

        public CrashReportFileProvider(ILogger<CrashReportFileProvider> logger, MainConnectionProvider connectionProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        }

        public async IAsyncEnumerable<string> FindMissingFilenames(string[] filenames, [EnumeratorCancellation] CancellationToken ct = default)
        {
            await using var connection = _connectionProvider.Connection;
            await connection.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(FindMissingFilenamesSql, connection);
            cmd.Parameters.Add(new NpgsqlParameter<string[]>("filenames", filenames));
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                yield return await reader.GetNullableStringAsync("filename", ct) ?? string.Empty;
            }
        }

        public async Task<CrashReportFileTableEntry?> UpsertAsync(CrashReportFileTableEntry crashReport, CancellationToken ct = default)
        {
            await using var connection = _connectionProvider.Connection;
            await connection.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(UpsertSql, connection);
            cmd.Parameters.Add(new NpgsqlParameter<string>("filename", crashReport.Filename));
            cmd.Parameters.Add(new NpgsqlParameter<Guid>("crash_report_id", crashReport.CrashReportId));
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            return await reader.ReadAsync(ct) ? await CreateCrashReportFileFromReaderAsync(reader, ct) : null;
        }

        private static async Task<CrashReportFileTableEntry> CreateCrashReportFileFromReaderAsync(DbDataReader reader, CancellationToken ct = default)
        {
            var filename = await reader.GetNullableStringAsync("filename", ct);
            var crashReportId = await reader.GetNullableGuidAsync("crash_report_id", ct);

            return new CrashReportFileTableEntry
            {
                Filename = filename ?? string.Empty,
                CrashReportId = crashReportId ?? Guid.Empty,
            };
        }
    }
}