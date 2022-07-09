using Npgsql;

using System;
using System.Collections.Immutable;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Extensions
{
    public static class NpgsqlDataReaderExtensions
    {
        public static async Task<Guid?> GetNullableGuidAsync(this DbDataReader reader, string columnName, CancellationToken ct = default)
        {
            var index = reader.GetOrdinal(columnName);
            return await reader.IsDBNullAsync(index, ct) ? null : reader.GetGuid(index);
        }
        public static async Task<float?> GetNullableFloatAsync(this DbDataReader reader, string columnName, CancellationToken ct = default)
        {
            var index = reader.GetOrdinal(columnName);
            return await reader.IsDBNullAsync(index, ct) ? null : reader.GetFloat(index);
        }
        public static async Task<decimal?> GetNullableDecimalAsync(this DbDataReader reader, string columnName, CancellationToken ct = default)
        {
            var index = reader.GetOrdinal(columnName);
            return await reader.IsDBNullAsync(index, ct) ? null : reader.GetDecimal(index);
        }
        public static async Task<string?> GetNullableStringAsync(this DbDataReader reader, string columnName, CancellationToken ct = default)
        {
            var index = reader.GetOrdinal(columnName);
            if (await reader.IsDBNullAsync(index, ct))
                return null;
            else
                return reader.GetString(index);
        }
        public static async Task<short?> GetNullableInt16Async(this DbDataReader reader, string columnName, CancellationToken ct = default)
        {
            var index = reader.GetOrdinal(columnName);
            return await reader.IsDBNullAsync(index, ct) ? null : reader.GetInt16(index);
        }
        public static async Task<int?> GetNullableInt32Async(this DbDataReader reader, string columnName, CancellationToken ct = default)
        {
            var index = reader.GetOrdinal(columnName);
            return await reader.IsDBNullAsync(index, ct) ? null : reader.GetInt32(index);
        }
        public static async Task<long?> GetNullableInt64Async(this DbDataReader reader, string columnName, CancellationToken ct = default)
        {
            var index = reader.GetOrdinal(columnName);
            return await reader.IsDBNullAsync(index, ct) ? null : reader.GetInt64(index);
        }
        public static DateTimeOffset GetTimeStamp(this DbDataReader reader, int ordinal)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(reader.GetInt64(ordinal));
        }
        public static async Task<DateTime?> GetNullableDateTimeAsync(this DbDataReader reader, string columnName, CancellationToken ct = default)
        {
            var index = reader.GetOrdinal(columnName);
            return await reader.IsDBNullAsync(index, ct) ? null : reader.GetDateTime(index);
        }
        public static async Task<ImmutableArray<string>?> GetNullableStringArrayAsync(this DbDataReader reader, string columnName, CancellationToken ct = default)
        {
            var index = reader.GetOrdinal(columnName);
            return await reader.IsDBNullAsync(index, ct) ? null : ImmutableArray.Create<string>(reader.GetFieldValue<string[]>(index));
        }
        public static async Task<ImmutableArray<int>?> GetNullableInt32ArrayAsync(this DbDataReader reader, string columnName, CancellationToken ct = default)
        {
            var index = reader.GetOrdinal(columnName);
            return await reader.IsDBNullAsync(index, ct) ? null : ImmutableArray.Create<int>(reader.GetFieldValue<int[]>(index));
        }
    }
}