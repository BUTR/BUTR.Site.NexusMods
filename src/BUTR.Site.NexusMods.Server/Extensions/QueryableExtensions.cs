using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Extensions
{
    public static class QueryableExtensions
    {
        public static Paging<TEntity> Paginated<TEntity>(this IQueryable<TEntity> queryable, uint page, uint pageSize) where TEntity : class
        {
            var count = queryable.Count();
            var items = queryable.Skip((int) ((page - 1) * pageSize)).Take((int) pageSize).ToImmutableArray();
            return new()
            {
                Items = items,
                Metadata = new()
                {
                    PageSize = pageSize,
                    CurrentPage = page,
                    TotalCount = (uint) count,
                    TotalPages = (uint) Math.Floor((double) count / (double) pageSize),
                }
            };
        }

        public static async Task<Paging<TEntity>> PaginatedAsync<TEntity>(this IQueryable<TEntity> queryable, uint page, uint pageSize, CancellationToken ct = default) where TEntity : class
        {
            var count = await queryable.CountAsync(ct);
            var items = await queryable.Skip((int) ((page - 1) * pageSize)).Take((int) pageSize).ToImmutableArrayAsync(ct);
            return new()
            {
                Items = items,
                Metadata = new()
                {
                    PageSize = pageSize,
                    CurrentPage = page,
                    TotalCount = (uint) count,
                    TotalPages = (uint) Math.Floor((double) count / (double) pageSize),
                }
            };
        }

        public static async Task<ImmutableArray<TSource>> ToImmutableArrayAsync<TSource>(this IQueryable<TSource> source, CancellationToken ct = default)
        {
            var builder = ImmutableArray.CreateBuilder<TSource>();
            await foreach (var element in source.AsAsyncEnumerable().WithCancellation(ct))
                builder.Add(element);
            return builder.ToImmutable();
        }
    }
}