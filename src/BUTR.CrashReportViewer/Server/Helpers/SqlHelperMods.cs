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
    public class SqlHelperMods
    {
        /// <summary>
        /// @val
        /// </summary>
        public static string ModsGetCountByUser = @"
SELECT
    COUNT(*)
FROM
    mod_entity
WHERE
    @val = ANY(user_ids)
;";

        /// <summary>
        /// @val, @offset, @limit
        /// </summary>
        public static string ModsGetPaginatedByUser = @"
SELECT
    *
FROM
    mod_entity
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
        public static string ModsGet = @"
SELECT
    *
FROM
    mod_entity
WHERE
    game_domain = @gameDomain AND mod_id = @modId
;";

        /// <summary>
        /// @gameDomain, @modId, @name, @userIds
        /// </summary>
        public static string ModsUpsert = @"
INSERT INTO mod_entity(game_domain, mod_id, name, user_ids)
VALUES (@gameDomain, @modId, @name, @userIds)
ON CONFLICT ON CONSTRAINT mod_entity_pkey
DO UPDATE SET user_ids=@userIds
RETURNING *
;";

        /// <summary>
        /// @userId
        /// </summary>
        public static string ModsGetModIdsByUserId = @"
SELECT
    ARRAY_AGG(mod_id) as mod_ids
FROM
    mod_entity
WHERE
    @userId = ANY(user_ids)
;";


        private readonly IConfiguration _configuration;

        public SqlHelperMods(IConfiguration configuration) => _configuration = configuration;

        public async Task<ModTableEntry?> FindAsync(string gameDomain, int modId, CancellationToken ct = default)
        {
            await using var connection = new NpgsqlConnection(_configuration.GetConnectionString("Main"));
            await connection.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(ModsGet, connection);
            cmd.Parameters.AddWithValue("gameDomain", gameDomain);
            cmd.Parameters.AddWithValue("modId", modId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            return await reader.ReadAsync(ct) ? await CreateModTableFromReaderAsync(reader, ct) : null;
        }

        public async Task<ModTableEntry?> UpsertAsync(ModTableEntry mod, CancellationToken ct = default)
        {
            await using var connection = new NpgsqlConnection(_configuration.GetConnectionString("Main"));
            await connection.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(ModsUpsert, connection);
            cmd.Parameters.AddWithValue("gameDomain", mod.GameDomain);
            cmd.Parameters.AddWithValue("modId", mod.ModId);
            cmd.Parameters.AddWithValue("name", mod.Name);
            cmd.Parameters.AddWithValue("userIds", mod.UserIds);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            return await reader.ReadAsync(ct) ? await CreateModTableFromReaderAsync(reader, ct) : null;
        }

        public async Task<(int, List<ModTableEntry>)> GetAsync(int userId, int skip, int take, CancellationToken ct = default)
        {
            static async Task<int> GetCount(NpgsqlDataReader reader, CancellationToken ct = default)
            {
                return await reader.ReadAsync(ct) ? reader.GetInt32(0) : 0;
            }
            static async IAsyncEnumerable<ModTableEntry> GetAllMods(NpgsqlDataReader reader, [EnumeratorCancellation] CancellationToken ct = default)
            {
                while (await reader.ReadAsync(ct))
                {
                    yield return await CreateModTableFromReaderAsync(reader, ct);
                }
            }

            await using var connection = new NpgsqlConnection(_configuration.GetConnectionString("Main"));
            await connection.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(
                ModsGetCountByUser +
                ModsGetPaginatedByUser, connection);
            cmd.Parameters.AddWithValue("val", userId);
            cmd.Parameters.AddWithValue("offset", skip);
            cmd.Parameters.AddWithValue("limit", take);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            var count = await GetCount(reader, ct);
            await reader.NextResultAsync(ct);
            var result = await GetAllMods(reader, ct).ToListAsync(ct);
            return (count, result);
        }

        private static async Task<ModTableEntry> CreateModTableFromReaderAsync(NpgsqlDataReader reader, CancellationToken ct = default)
        {
            var gameDomain = await reader.GetNullableStringAsync("game_domain", ct);
            var modId = await reader.GetNullableInt32Async("mod_id", ct);
            var name = await reader.GetNullableStringAsync("name", ct);
            var userIds = await reader.GetNullableInt32ArrayAsync("user_ids", ct);

            return new ModTableEntry
            {
                GameDomain = gameDomain ?? string.Empty,
                ModId = modId ?? 0,
                Name = name ?? string.Empty,
                UserIds = userIds ?? Array.Empty<int>()
            };
        }
    }
}