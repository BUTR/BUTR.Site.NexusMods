using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Utils.Npgsql;

using DynamicExpressions;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Extensions;

public static class QueryableExtensions
{
    public static async IAsyncEnumerable<ImmutableArray<T>> BatchedAsync<T>(this IQueryable<T> query, int batchSize = 3000)
    {
        var processed = 0;

        Task<ImmutableArray<T>> FetchNext() => query.Skip(processed).Take(batchSize).ToImmutableArrayAsync();
        var fetch = FetchNext();

        while (true)
        {
            var toProcess = await fetch;
            if (toProcess.Length == 0)
                yield break;
            processed += toProcess.Length;

            // Start pre-fetching the next batch if there's still available
            fetch = toProcess.Length == batchSize ? FetchNext() : Task.FromResult(ImmutableArray<T>.Empty);

            yield return toProcess;
        }
    }

    public static Task<Paging<TEntity>> PaginatedAsync<TEntity>(this IQueryable<TEntity> queryable, PaginatedQuery query, uint maxPageSize = 20, Sorting? defaultSorting = default, CancellationToken ct = default) where TEntity : class
    {
        var page = query.Page;
        var pageSize = Math.Max(Math.Min(query.PageSize, maxPageSize), 5);
        var filters = query.Filters ?? Enumerable.Empty<Filtering>();
        var sortings = query.Sotings is null || query.Sotings.Count == 0
            ? defaultSorting == null ? Array.Empty<Sorting>() : new List<Sorting> { defaultSorting }
            : query.Sotings;

        return queryable
            .WithFilter(filters)
            .WithSort(sortings)
            .PaginatedAsync(page, pageSize, ct);
    }

    // This implementation will introduce a little desync of count because items are lazily evaluated
    public static async Task<Paging<TEntity>> PaginatedAsync<TEntity>(this IQueryable<TEntity> queryable, uint page, uint pageSize, CancellationToken ct = default) where TEntity : class
    {
        var startTime = Stopwatch.GetTimestamp();
        var count = await queryable.CountAsync(ct);

        return new()
        {
            StartTime = startTime,
            Items = queryable.Skip((int) ((page - 1) * pageSize)).Take((int) pageSize).AsNoTracking().AsAsyncEnumerable(),
            Metadata = new()
            {
                PageSize = pageSize,
                CurrentPage = page,
                TotalCount = (uint) count,
                TotalPages = (uint) Math.Floor((double) count / (double) pageSize),
            }
        };
    }

    public static Task<ImmutableArray<TSource>> ToImmutableArrayAsync<TSource>(this IQueryable<TSource> source, CancellationToken ct = default)
    {
        return source.AsAsyncEnumerable().ToImmutableArrayAsync(ct);
    }


    private static FilterOperator Convert(FilteringType filteringType) => filteringType switch
    {
        FilteringType.Equal => FilterOperator.Equals,
        FilteringType.NotEquals => FilterOperator.DoesntEqual,
        FilteringType.GreaterThan => FilterOperator.GreaterThan,
        FilteringType.GreaterThanOrEqual => FilterOperator.GreaterThanOrEqual,
        FilteringType.LessThan => FilterOperator.LessThan,
        FilteringType.LessThanOrEqual => FilterOperator.LessThanOrEqual,
        FilteringType.Contains => FilterOperator.Contains,
        FilteringType.NotContains => FilterOperator.NotContains,
        FilteringType.StartsWith => FilterOperator.StartsWith,
        FilteringType.EndsWith => FilterOperator.EndsWith,
        _ => throw new ArgumentOutOfRangeException(nameof(filteringType), filteringType, null)
    };

    private static bool TryConvertValue(Type type, string rawValue, [NotNullWhen((true))] out object? value)
    {
        if (type.IsEnum)
            return Enum.TryParse(type, rawValue, out value);

        if (type.IsArray)
            type = type.GetElementType()!;

        if (type is { IsGenericType: true, GenericTypeArguments.Length: 1 })
            type = type.GenericTypeArguments[0];

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.String:
            {
                value = rawValue;
                return true;
            }
            case TypeCode.Byte:
            {
                var result = byte.TryParse(rawValue, out var val);
                value = val;
                return result;
            }
            case TypeCode.SByte:
            {
                var result = sbyte.TryParse(rawValue, out var val);
                value = val;
                return result;
            }
            case TypeCode.UInt16:
            {
                var result = ushort.TryParse(rawValue, out var val);
                value = val;
                return result;
            }
            case TypeCode.UInt32:
            {
                var result = uint.TryParse(rawValue, out var val);
                value = val;
                return result;
            }
            case TypeCode.UInt64:
            {
                var result = ulong.TryParse(rawValue, out var val);
                value = val;
                return result;
            }
            case TypeCode.Int16:
            {
                var result = short.TryParse(rawValue, out var val);
                value = val;
                return result;
            }
            case TypeCode.Int32:
            {
                var result = int.TryParse(rawValue, out var val);
                value = val;
                return result;
            }
            case TypeCode.Int64:
            {
                var result = long.TryParse(rawValue, out var val);
                value = val;
                return result;
            }
            case TypeCode.Decimal:
            {
                var result = decimal.TryParse(rawValue, out var val);
                value = val;
                return result;
            }
            case TypeCode.Double:
            {
                var result = double.TryParse(rawValue, out var val);
                value = val;
                return result;
            }
            case TypeCode.Single:
            {
                var result = float.TryParse(rawValue, out var val);
                value = val;
                return result;
            }
            case TypeCode.Boolean:
            {
                var result = bool.TryParse(rawValue, out var val);
                value = val;
                return result;
            }
            case TypeCode.Char:
            {
                value = rawValue[0];
                return true;
            }
            case TypeCode.DateTime:
            {
                var result = DateTime.TryParse(rawValue, null, DateTimeStyles.RoundtripKind, out var val);
                value = DateTime.SpecifyKind(val, DateTimeKind.Utc);
                return result;
            }
            default:
            {
                value = null;
                return false;
            }
        }
    }

    private static Expression<Func<TEntity, bool>>? GetFilteringPredicate<TEntity>(Filtering filter)
    {
        if (typeof(TEntity).GetProperty(filter.Property) is not { } propertyInfo)
            return null;

        if (!TryConvertValue(propertyInfo.PropertyType, filter.Value, out var converted))
            return null;

        return DynamicExpressions.DynamicExpressions.GetPredicate<TEntity>(filter.Property, Convert(filter.Type), converted);
    }

    public static IQueryable<TEntity> WithFilter<TEntity>(this IQueryable<TEntity> queryable, IEnumerable<Filtering> filters)
    {
        foreach (var filter in filters.Select(GetFilteringPredicate<TEntity>).OfType<Expression<Func<TEntity, bool>>>())
            queryable = queryable.Where(filter);

        return queryable;
    }

    private static Expression<Func<TEntity, object>>? GetOrderPredicate<TEntity>(Sorting sorting)
    {
        if (typeof(TEntity).GetProperty(sorting.Property) is null)
            return null;

        return DynamicExpressions.DynamicExpressions.GetPropertyGetter<TEntity>(sorting.Property);
    }

    public static IQueryable<TEntity> WithSort<TEntity>(this IQueryable<TEntity> queryable, IEnumerable<Sorting> sortings)
    {
        IOrderedQueryable<TEntity>? ordered = null;
        foreach (var (sorting, predicate) in sortings.Select(x => (x, GetOrderPredicate<TEntity>(x))))
        {
            if (predicate is null)
                continue;

            if (ordered is null)
                ordered = sorting.Type == SortingType.Ascending ? queryable.OrderBy(predicate) : queryable.OrderByDescending(predicate);
            else
                ordered = sorting.Type == SortingType.Ascending ? ordered.ThenBy(predicate) : ordered.ThenByDescending(predicate);
        }

        return ordered ?? queryable;
    }

    public static IQueryable<T> Prepare<T>(this IQueryable<T> query)
    {
        return query.TagWith(PrepareCommandInterceptor.Tag);
    }
}