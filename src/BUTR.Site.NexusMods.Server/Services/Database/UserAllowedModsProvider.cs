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
    public sealed class UserAllowedModsProvider
    {
        /// <summary>
        /// @val
        /// </summary>
        private static string GetCountSql = @"
SELECT
    COUNT(*)
FROM
    user_allowed_mods
;";

        /// <summary>
        /// @val, @offset, @limit
        /// </summary>
        private static string GetPaginatedSql = @"
SELECT
    *
FROM
    user_allowed_mods
ORDER BY
    user_id
OFFSET
    @offset
LIMIT
    @limit
;";

        /// <summary>
        /// @userId
        /// </summary>
        private static string GetSql = @"
SELECT
    umi.*
FROM
    user_allowed_mods umi
WHERE
    umi.user_id = @userId
;";

        /// <summary>
        /// @userId, @allowedModIds
        /// </summary>
        private static string UpsertSql = @"
INSERT INTO user_allowed_mods(user_id, allowed_mod_ids)
VALUES (@userId, @allowedModIds)
ON CONFLICT ON CONSTRAINT user_allowed_mods_pkey
DO UPDATE SET allowed_mod_ids = @allowedModIds
RETURNING *
;";


        private readonly ILogger _logger;
        private readonly MainConnectionProvider _connectionProvider;

        public UserAllowedModsProvider(ILogger<UserAllowedModsProvider> logger, MainConnectionProvider connectionProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        }

        public async Task<UserAllowedModsTableEntry?> FindAsync(int userId, CancellationToken ct = default)
        {
            await using var connection = _connectionProvider.Connection;
            await connection.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(GetSql, connection);
            cmd.Parameters.Add(new NpgsqlParameter<int>("userId", userId));
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return await CreateUserAllowedModsFromReaderAsync(reader, ct);
            }

            return null;
        }

        public async Task<UserAllowedModsTableEntry?> UpsertAsync(UserAllowedModsTableEntry entry, CancellationToken ct = default)
        {
            await using var connection = _connectionProvider.Connection;
            await connection.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(UpsertSql, connection);
            cmd.Parameters.Add(new NpgsqlParameter<int>("userId", entry.UserId));
            cmd.Parameters.Add(new NpgsqlParameter<ImmutableArray<string>>("allowedModIds", entry.AllowedModIds));
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return await CreateUserAllowedModsFromReaderAsync(reader, ct);
            }

            return null;
        }

        public async Task<(int, List<UserAllowedModsTableEntry>)> GetPaginatedAsync(int skip, int take, CancellationToken ct = default)
        {
            static async Task<int> GetCount(DbDataReader reader, CancellationToken ct = default)
            {
                return await reader.ReadAsync(ct) ? reader.GetInt32(0) : 0;
            }
            static async IAsyncEnumerable<UserAllowedModsTableEntry> GetAllMods(DbDataReader reader, [EnumeratorCancellation] CancellationToken ct = default)
            {
                while (await reader.ReadAsync(ct))
                {
                    yield return await CreateUserAllowedModsFromReaderAsync(reader, ct);
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

        private static async Task<UserAllowedModsTableEntry> CreateUserAllowedModsFromReaderAsync(DbDataReader reader, CancellationToken ct = default)
        {
            var userId = await reader.GetNullableInt32Async("user_id", ct);
            var allowedModIds = await reader.GetNullableStringArrayAsync("allowed_mod_ids", ct);

            return new UserAllowedModsTableEntry
            {
                UserId = userId ?? -1,
                AllowedModIds = allowedModIds ?? ImmutableArray<string>.Empty,
            };
        }
    }
}