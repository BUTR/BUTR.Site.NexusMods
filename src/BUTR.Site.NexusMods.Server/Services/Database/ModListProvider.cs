using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Npgsql;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services.Database
{
    public class ModListProvider
    {
        /// <summary>
        /// @val
        /// </summary>
        public static string ModListGetCountByName = @"
SELECT
    COUNT(*)
FROM
    modlist_entity
WHERE
    @val LIKE name
;";

        /// <summary>
        /// @val, @offset, @limit
        /// </summary>
        public static string ModListGetPaginatedByName = @"
SELECT
    *
FROM
    modlist_entity
WHERE
    @val LIKE name
ORDER BY
    name
OFFSET
    @offset
LIMIT
    @limit
;";

        /// <summary>
        /// @name
        /// </summary>
        public static string ModListGet = @"
SELECT
    *
FROM
    modlist_entity
WHERE
    @name LIKE name
;";

        /// <summary>
        /// @id, @name, @userId, @content
        /// </summary>
        public static string ModListUpsert = @"
INSERT INTO modlist_entity(id, name, userId, content)
VALUES (@id, @name, @userId, @content)
ON CONFLICT ON CONSTRAINT modlist_entity_pkey
DO UPDATE SET content=@content
RETURNING *
;";

        /// <summary>
        /// @id
        /// </summary>
        public static string ModListDelete = @"
DELETE FROM modlist_entity
WHERE
    @id LIKE id
;";

        /// <summary>
        /// @userId
        /// </summary>
        public static string ModsGetByUserId = @"
SELECT
    *
FROM
    modlist_entity
WHERE
    @userId = user_id
;";


        private readonly ILogger _logger;
        private readonly ConnectionStringsOptions _options;

        public ModListProvider(ILogger<ModListProvider> logger, IOptions<ConnectionStringsOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options.Value ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ModListEntry?> FindAsync(string name, int userId, CancellationToken ct = default)
        {
            await using var connection = new NpgsqlConnection(_options.Main);
            await connection.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(ModListGet, connection);
            cmd.Parameters.AddWithValue("name", name);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            return await reader.ReadAsync(ct) ? await CreateModListFromReaderAsync(reader, ct) : null;
        }

        public async Task<ModListEntry?> UpsertAsync(ModListEntry mod, CancellationToken ct = default)
        {
            await using var connection = new NpgsqlConnection(_options.Main);
            await connection.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(ModListUpsert, connection);
            cmd.Parameters.AddWithValue("id", mod.Id);
            cmd.Parameters.AddWithValue("name", mod.Name);
            cmd.Parameters.AddWithValue("userId", mod.UserId);
            cmd.Parameters.AddWithValue("content", mod.Content);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            return await reader.ReadAsync(ct) ? await CreateModListFromReaderAsync(reader, ct) : null;
        }

        public async Task<bool> DeleteAsync(ModListEntry mod, CancellationToken ct = default)
        {
            await using var connection = new NpgsqlConnection(_options.Main);
            await connection.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(ModListDelete, connection);
            cmd.Parameters.AddWithValue("id", mod.Id);
            var result = await cmd.ExecuteNonQueryAsync(ct);
            return result > 0;
        }

        public async Task<(int, List<ModListEntry>)> GetPaginatedAsync(int userId, string name, int skip, int take, CancellationToken ct = default)
        {
            static async Task<int> GetCount(NpgsqlDataReader reader, CancellationToken ct = default)
            {
                return await reader.ReadAsync(ct) ? reader.GetInt32(0) : 0;
            }
            static async IAsyncEnumerable<ModListEntry> GetAllMods(NpgsqlDataReader reader, [EnumeratorCancellation] CancellationToken ct = default)
            {
                while (await reader.ReadAsync(ct))
                {
                    yield return await CreateModListFromReaderAsync(reader, ct);
                }
            }

            await using var connection = new NpgsqlConnection(_options.Main);
            await connection.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(
                ModListGetCountByName +
                ModListGetPaginatedByName, connection);
            cmd.Parameters.AddWithValue("val", userId);
            cmd.Parameters.AddWithValue("offset", skip);
            cmd.Parameters.AddWithValue("limit", take);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            var count = await GetCount(reader, ct);
            await reader.NextResultAsync(ct);
            var result = await GetAllMods(reader, ct).ToListAsync(ct);
            return (count, result);
        }

        private static async Task<ModListEntry> CreateModListFromReaderAsync(NpgsqlDataReader reader, CancellationToken ct = default)
        {
            var id = await reader.GetNullableGuidAsync("id", ct);
            var name = await reader.GetNullableStringAsync("name", ct);
            var userId = await reader.GetNullableInt32Async("user_id", ct);
            var content = await reader.GetNullableStringAsync("content", ct);

            return new ModListEntry
            {
                Id = id ?? Guid.Empty,
                Name = name ?? string.Empty,
                UserId = userId ?? 0,
                Content = content ?? string.Empty,
            };
        }
    }
}