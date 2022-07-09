using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Npgsql;

using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services.Database
{
    public sealed class UserRoleProvider
    {
        /// <summary>
        /// @user_id
        /// </summary>
        private static readonly string GetSql = @"
SELECT
    *
FROM
    user_role
WHERE
    user_id = @user_id
;";

        /// <summary>
        /// @user_id, @role
        /// </summary>
        private static readonly string UpsertSql = @"
INSERT INTO user_role(user_id, role)
VALUES (@user_id, @role)
ON CONFLICT ON CONSTRAINT user_role_pkey
DO UPDATE SET role = @role
RETURNING *
;";

        /// <summary>
        /// @user_id
        /// </summary>
        private static readonly string DeleteSql = @"
DELETE FROM user_role
WHERE
    @user_id = user_id
;";

        private readonly ILogger _logger;
        private readonly ConnectionStringsOptions _options;

        public UserRoleProvider(ILogger<UserRoleProvider> logger, IOptions<ConnectionStringsOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options.Value ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<UserRoleTableEntry?> FindAsync(int userId, CancellationToken ct = default)
        {
            await using var connection = new NpgsqlConnection(_options.Main);
            await connection.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(GetSql, connection);
            cmd.Parameters.Add(new NpgsqlParameter<int>("user_id", userId));
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            return await reader.ReadAsync(ct) ? await CreateUserRoleFromReaderAsync(reader, ct) : null;
        }

        public async Task<UserRoleTableEntry?> UpsertAsync(UserRoleTableEntry mod, CancellationToken ct = default)
        {
            await using var connection = new NpgsqlConnection(_options.Main);
            await connection.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(UpsertSql, connection);
            cmd.Parameters.Add(new NpgsqlParameter<int>("user_id", mod.UserId));
            cmd.Parameters.Add(new NpgsqlParameter<string>("role", mod.Role));
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            return await reader.ReadAsync(ct) ? await CreateUserRoleFromReaderAsync(reader, ct) : null;
        }

        public async Task<bool> DeleteAsync(UserRoleTableEntry mod, CancellationToken ct = default)
        {
            await using var connection = new NpgsqlConnection(_options.Main);
            await connection.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(DeleteSql, connection);
            cmd.Parameters.Add(new NpgsqlParameter<int>("user_id", mod.UserId));
            var result = await cmd.ExecuteNonQueryAsync(ct);
            return result > 0;
        }

        private static async Task<UserRoleTableEntry> CreateUserRoleFromReaderAsync(DbDataReader reader, CancellationToken ct = default)
        {
            var userId = await reader.GetNullableInt32Async("user_id", ct);
            var role = await reader.GetNullableStringAsync("role", ct);

            return new UserRoleTableEntry
            {
                UserId = userId ?? -1,
                Role = role ?? string.Empty,
            };
        }
    }
}