using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.Extensions.Logging;

using Npgsql;

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services.Database
{
    public sealed class ModNexusModsManualLinkProvider
    {
        /// <summary>
        /// @val
        /// </summary>
        private static string GetCountSql = @"
SELECT
    COUNT(*)
FROM
    mod_nexusmods_manual_link
;";

        /// <summary>
        /// @val, @offset, @limit
        /// </summary>
        private static string GetPaginatedSql = @"
SELECT
    *
FROM
    mod_nexusmods_manual_link
ORDER BY
    mod_id
OFFSET
    @offset
LIMIT
    @limit
;";

        /// <summary>
        /// @modId
        /// </summary>
        private static string GetSql = @"
SELECT
    mitnmi.*
FROM
    mod_nexusmods_manual_link mitnmi
WHERE
    mitnmi.mod_id = @modId
;";

        /// <summary>
        /// @modId, @nexusModsId
        /// </summary>
        private static string UpsertSql = @"
INSERT INTO mod_nexusmods_manual_link(mod_id, nexusmods_id)
VALUES (@modId, @nexusModsId)
ON CONFLICT ON CONSTRAINT mod_nexus_mods_manual_link_pkey
DO UPDATE SET nexusmods_id = @nexusModsId
RETURNING *
;";

        private readonly ILogger _logger;
        private readonly MainConnectionProvider _connectionProvider;

        public ModNexusModsManualLinkProvider(ILogger<ModNexusModsManualLinkProvider> logger, MainConnectionProvider connectionProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        }

        public async Task<ModNexusModsManualLinkTableEntry?> FindAsync(string modId, CancellationToken ct = default)
        {
            await using var connection = _connectionProvider.Connection;
            await connection.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(GetSql, connection);
            cmd.Parameters.Add(new NpgsqlParameter<string>("modId", modId));
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return await CreateModNexusModsManualLinkFromReaderAsync(reader, ct);
            }

            return null;
        }

        public async Task<ModNexusModsManualLinkTableEntry?> UpsertAsync(ModNexusModsManualLinkTableEntry entry, CancellationToken ct = default)
        {
            await using var connection = _connectionProvider.Connection;
            await connection.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(UpsertSql, connection);
            cmd.Parameters.Add(new NpgsqlParameter<string>("modId", entry.ModId));
            cmd.Parameters.Add(new NpgsqlParameter<int>("nexusModsId", entry.NexusModsId));
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return await CreateModNexusModsManualLinkFromReaderAsync(reader, ct);
            }

            return null;
        }

        public async Task<(int, List<ModNexusModsManualLinkTableEntry>)> GetPaginatedAsync(int skip, int take, CancellationToken ct = default)
        {
            static async Task<int> GetCount(DbDataReader reader, CancellationToken ct = default)
            {
                return await reader.ReadAsync(ct) ? reader.GetInt32(0) : 0;
            }
            static async IAsyncEnumerable<ModNexusModsManualLinkTableEntry> GetAllMods(DbDataReader reader, [EnumeratorCancellation] CancellationToken ct = default)
            {
                while (await reader.ReadAsync(ct))
                {
                    yield return await CreateModNexusModsManualLinkFromReaderAsync(reader, ct);
                }
            }

            await using var connection = _connectionProvider.Connection;
            await connection.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(
                GetCountSql +
                GetPaginatedSql, connection);
            cmd.Parameters.Add(new NpgsqlParameter<int>("offset", skip));
            cmd.Parameters.Add(new NpgsqlParameter<int>("limit", take));
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            var count = await GetCount(reader, ct);
            await reader.NextResultAsync(ct);
            var result = await GetAllMods(reader, ct).ToListAsync(ct);
            return (count, result);
        }

        private static async Task<ModNexusModsManualLinkTableEntry> CreateModNexusModsManualLinkFromReaderAsync(DbDataReader reader, CancellationToken ct = default)
        {
            var modId = await reader.GetNullableStringAsync("mod_id", ct);
            var nexusModsId = await reader.GetNullableInt32Async("nexusmods_id", ct);

            return new ModNexusModsManualLinkTableEntry
            {
                ModId = modId ?? string.Empty,
                NexusModsId = nexusModsId ?? -1,
            };
        }
    }
}