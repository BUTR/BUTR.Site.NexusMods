using Npgsql;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Extensions
{
    public static class NpgsqlDataReaderExtensions
    {
        public static async Task<Guid?> GetNullableGuidAsync(this NpgsqlDataReader reader, string columnName, CancellationToken ct = default)
        {
            var index = reader.GetOrdinal(columnName);
            return await reader.IsDBNullAsync(index, ct) ? null : reader.GetGuid(index);
        }
        public static async Task<float?> GetNullableFloatAsync(this NpgsqlDataReader reader, string columnName, CancellationToken ct = default)
        {
            var index = reader.GetOrdinal(columnName);
            return await reader.IsDBNullAsync(index, ct) ? null : reader.GetFloat(index);
        }
        public static async Task<decimal?> GetNullableDecimalAsync(this NpgsqlDataReader reader, string columnName, CancellationToken ct = default)
        {
            var index = reader.GetOrdinal(columnName);
            return await reader.IsDBNullAsync(index, ct) ? null : reader.GetDecimal(index);
        }
        public static async Task<string?> GetNullableStringAsync(this NpgsqlDataReader reader, string columnName, CancellationToken ct = default)
        {
            var index = reader.GetOrdinal(columnName);
            if (await reader.IsDBNullAsync(index, ct))
                return null;
            else
                return reader.GetString(index);
        }
        public static async Task<short?> GetNullableInt16Async(this NpgsqlDataReader reader, string columnName, CancellationToken ct = default)
        {
            var index = reader.GetOrdinal(columnName);
            return await reader.IsDBNullAsync(index, ct) ? null : reader.GetInt16(index);
        }
        public static async Task<int?> GetNullableInt32Async(this NpgsqlDataReader reader, string columnName, CancellationToken ct = default)
        {
            var index = reader.GetOrdinal(columnName);
            return await reader.IsDBNullAsync(index, ct) ? null : reader.GetInt32(index);
        }
        public static async Task<long?> GetNullableInt64Async(this NpgsqlDataReader reader, string columnName, CancellationToken ct = default)
        {
            var index = reader.GetOrdinal(columnName);
            return await reader.IsDBNullAsync(index, ct) ? null : reader.GetInt64(index);
        }
        public static async Task<DateTime?> GetNullableTimeStampAsync(this NpgsqlDataReader reader, string columnName, CancellationToken ct = default)
        {
            var index = reader.GetOrdinal(columnName);
            return await reader.IsDBNullAsync(index, ct) ? null : reader.GetTimeStamp(index).ToDateTime();
        }
        public static async Task<int[]?> GetNullableInt32ArrayAsync(this NpgsqlDataReader reader, string columnName, CancellationToken ct = default)
        {
            var index = reader.GetOrdinal(columnName);
            return await reader.IsDBNullAsync(index, ct) ? null : reader.GetFieldValue<int[]>(index);
        }
    }
}