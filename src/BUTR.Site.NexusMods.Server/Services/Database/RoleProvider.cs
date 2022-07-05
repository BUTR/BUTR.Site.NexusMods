using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Npgsql;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services.Database
{
    public class RoleProvider
    {
        /// <summary>
        /// @user_id
        /// </summary>
        private static readonly string RoleGet = @"
SELECT
    *
FROM
    role_entity
WHERE
    user_id = @user_id
;";

        /// <summary>
        /// @user_id, @role
        /// </summary>
        private static readonly string RoleUpsert = @"
INSERT INTO role_entity(user_id, role)
VALUES (@user_id, @role)
ON CONFLICT ON CONSTRAINT role_entity_pkey
DO UPDATE SET role = @role
RETURNING *
;";

        /// <summary>
        /// @user_id
        /// </summary>
        private static readonly string RoleDelete = @"
DELETE FROM role_entity
WHERE
    @user_id = user_id
;";

        private readonly ILogger _logger;
        private readonly ConnectionStringsOptions _options;

        public RoleProvider(ILogger<RoleProvider> logger, IOptions<ConnectionStringsOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options.Value ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<RoleEntry?> FindAsync(uint userId, CancellationToken ct = default)
        {
            await using var connection = new NpgsqlConnection(_options.Main);
            await connection.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(RoleGet, connection);
            cmd.Parameters.AddWithValue("user_id", (int) userId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            return await reader.ReadAsync(ct) ? await CreateRoleFromReaderAsync(reader, ct) : null;
        }

        public async Task<RoleEntry?> UpsertAsync(RoleEntry mod, CancellationToken ct = default)
        {
            await using var connection = new NpgsqlConnection(_options.Main);
            await connection.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(RoleUpsert, connection);
            cmd.Parameters.AddWithValue("user_id", (int) mod.UserId);
            cmd.Parameters.AddWithValue("role", mod.Role);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            return await reader.ReadAsync(ct) ? await CreateRoleFromReaderAsync(reader, ct) : null;
        }

        public async Task<bool> DeleteAsync(RoleEntry mod, CancellationToken ct = default)
        {
            await using var connection = new NpgsqlConnection(_options.Main);
            await connection.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(RoleDelete, connection);
            cmd.Parameters.AddWithValue("user_id", (int) mod.UserId);
            var result = await cmd.ExecuteNonQueryAsync(ct);
            return result > 0;
        }

        private static async Task<RoleEntry> CreateRoleFromReaderAsync(NpgsqlDataReader reader, CancellationToken ct = default)
        {
            var userId = await reader.GetNullableInt32Async("user_id", ct);
            var role = await reader.GetNullableStringAsync("role", ct);

            return new RoleEntry
            {
                UserId = (uint?) userId ?? 0U,
                Role = role ?? string.Empty,
            };
        }
    }
}