using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.Extensions.Logging;

using Npgsql;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services.Database
{
    public sealed class NexusModsModProvider
    {
        /// <summary>
        /// @val
        /// </summary>
        private static string GetCountByUserSql = @"
SELECT
    COUNT(*)
FROM
    nexusmods_mod_entity
WHERE
    @val = ANY(user_ids)
;";

        /// <summary>
        /// @val, @offset, @limit
        /// </summary>
        private static string GetPaginatedByUserSql = @"
SELECT
    *
FROM
    nexusmods_mod_entity
WHERE
    @val = ANY(user_ids)
ORDER BY
    mod_id
OFFSET
    @offset
LIMIT
    @limit
;";

        /// <summary>
        /// @gameDomain, @modId
        /// </summary>
        private static string GetSql = @"
SELECT
    *
FROM
    nexusmods_mod_entity
WHERE
    mod_id = @modId
;";

        /// <summary>
        /// @modId, @name, @userIds
        /// </summary>
        private static string UpsertSql = @"
INSERT INTO nexusmods_mod_entity(mod_id, name, user_ids)
VALUES (@modId, @name, @userIds)
ON CONFLICT ON CONSTRAINT nexusmods_mod_entity_pkey
DO UPDATE SET user_ids = @userIds
RETURNING *
;";

        /// <summary>
        /// @userId
        /// </summary>
        private static string GetModIdsByUserIdSql = @"
SELECT
    ARRAY_AGG(mod_id) as mod_ids
FROM
    nexusmods_mod_entity
WHERE
    @userId = ANY(user_ids)
;";


        private readonly ILogger _logger;
        private readonly MainConnectionProvider _connectionProvider;

        public NexusModsModProvider(ILogger<NexusModsModProvider> logger, MainConnectionProvider connectionProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        }

        public async Task<NexusModsModTableEntry?> FindAsync(int modId, CancellationToken ct = default)
        {
            await using var connection = _connectionProvider.Connection;
            await connection.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(GetSql, connection);
            cmd.Parameters.Add(new NpgsqlParameter<int>("modId", modId));
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            return await reader.ReadAsync(ct) ? await CreateModFromReaderAsync(reader, ct) : null;
        }

        public async Task<NexusModsModTableEntry?> UpsertAsync(NexusModsModTableEntry nexusModsMod, CancellationToken ct = default)
        {
            await using var connection = _connectionProvider.Connection;
            await connection.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(UpsertSql, connection);
            cmd.Parameters.Add(new NpgsqlParameter<int>("modId", nexusModsMod.ModId));
            cmd.Parameters.Add(new NpgsqlParameter<string>("name", nexusModsMod.Name));
            cmd.Parameters.Add(new NpgsqlParameter<ImmutableArray<int>>("userIds", nexusModsMod.UserIds));
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            return await reader.ReadAsync(ct) ? await CreateModFromReaderAsync(reader, ct) : null;
        }

        public async Task<(int, List<NexusModsModTableEntry>)> GetPaginatedAsync(int userId, int skip, int take, CancellationToken ct = default)
        {
            static async Task<int> GetCount(DbDataReader reader, CancellationToken ct = default)
            {
                return await reader.ReadAsync(ct) ? reader.GetInt32(0) : 0;
            }
            static async IAsyncEnumerable<NexusModsModTableEntry> GetAllMods(DbDataReader reader, [EnumeratorCancellation] CancellationToken ct = default)
            {
                while (await reader.ReadAsync(ct))
                {
                    yield return await CreateModFromReaderAsync(reader, ct);
                }
            }

            await using var connection = _connectionProvider.Connection;
            await connection.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(
                GetCountByUserSql +
                GetPaginatedByUserSql, connection);
            cmd.Parameters.Add(new NpgsqlParameter<int>("val", userId));
            cmd.Parameters.Add(new NpgsqlParameter<int>("offset", skip));
            cmd.Parameters.Add(new NpgsqlParameter<int>("limit", take));
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            var count = await GetCount(reader, ct);
            await reader.NextResultAsync(ct);
            var result = await GetAllMods(reader, ct).ToListAsync(ct);
            return (count, result);
        }

        private static async Task<NexusModsModTableEntry> CreateModFromReaderAsync(DbDataReader reader, CancellationToken ct = default)
        {
            var modId = await reader.GetNullableInt32Async("mod_id", ct);
            var name = await reader.GetNullableStringAsync("name", ct);
            var userIds = await reader.GetNullableInt32ArrayAsync("user_ids", ct);

            return new NexusModsModTableEntry
            {
                ModId = modId ?? 0,
                Name = name ?? string.Empty,
                UserIds = userIds ?? ImmutableArray<int>.Empty
            };
        }
    }
}