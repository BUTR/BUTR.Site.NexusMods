﻿using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;

using DynamicExpressions;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Extensions;

public static class QueryableExtensions
{
    private static readonly IQueryable<string> Empty = Enumerable.Empty<string>().AsQueryable();

    public static IQueryable<string> AutocompleteStartsWith<TEntity, TProperty>(this AppDbContext dbContext, Expression<Func<TEntity, TProperty>> property, string value)
    {
        if (property is not LambdaExpression { Body: MemberExpression { Member: PropertyInfo propertyInfo } }) return Empty;
        if (!property.Type.IsGenericType || property.Type.GenericTypeArguments.Length != 2) return Empty;

        var autocompleteEntity = dbContext.Model.FindEntityType(typeof(AutocompleteEntity))!;
        var autocompleteEntityTable = autocompleteEntity.GetSchemaQualifiedTableName();
        var typeProperty = autocompleteEntity.GetProperty(nameof(AutocompleteEntity.Type)).GetColumnName();
        var valuesProperty = autocompleteEntity.GetProperty(nameof(AutocompleteEntity.Values)).GetColumnName();

        var entityType = property.Type.GenericTypeArguments[0];
        var name = $"{entityType.Name}.{propertyInfo.Name}";

        var tableNameParam = new NpgsqlParameter<string>("tableName", name);
        var valParam = new NpgsqlParameter<string>("val", value);
        return dbContext.Set<AutocompleteEntity>().FromSqlRaw(@$"
WITH values AS (SELECT unnest({valuesProperty}) as value FROM {autocompleteEntityTable} WHERE type = @tableName ORDER BY {valuesProperty})
SELECT
    value as {typeProperty}
FROM
    values
WHERE
    value ILIKE @val || '%'", tableNameParam, valParam).Select(x => x.Type);
    }

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
                TotalPages = (uint) Math.Floor((double) count / (double) pageSize)
            }
        };
    }

    public static async Task<ImmutableArray<TSource>> ToImmutableArrayAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken ct = default)
    {
        var builder = ImmutableArray.CreateBuilder<TSource>();
        await foreach (var element in source.AsAsyncEnumerable().WithCancellation(ct))
            builder.Add(element);
        return builder.ToImmutable();
    }

    public static async Task<ImmutableArray<TSource>> ToImmutableArrayAsync<TSource>(this IQueryable<TSource> source, CancellationToken ct = default)
    {
        var builder = ImmutableArray.CreateBuilder<TSource>();
        await foreach (var element in source.AsAsyncEnumerable().WithCancellation(ct))
            builder.Add(element);
        return builder.ToImmutable();
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

    private static object? ConvertValue(Type type, string rawValue)
    {
        if (type.IsEnum)
            return Enum.Parse(type, rawValue);

        if (type.IsArray)
            type = type.GetElementType()!;

        if (type is { IsGenericType: true, GenericTypeArguments.Length: 1})
            type = type.GenericTypeArguments[0];

        return Type.GetTypeCode(type) switch
        {
            TypeCode.String => rawValue,
            TypeCode.Byte => byte.Parse(rawValue),
            TypeCode.SByte => sbyte.Parse(rawValue),
            TypeCode.UInt16 => ushort.Parse(rawValue),
            TypeCode.UInt32 => uint.Parse(rawValue),
            TypeCode.UInt64 => ulong.Parse(rawValue),
            TypeCode.Int16 => short.Parse(rawValue),
            TypeCode.Int32 => int.Parse(rawValue),
            TypeCode.Int64 => long.Parse(rawValue),
            TypeCode.Decimal => decimal.Parse(rawValue),
            TypeCode.Double => double.Parse(rawValue),
            TypeCode.Single => float.Parse(rawValue),
            TypeCode.Boolean => bool.Parse(rawValue),
            TypeCode.Char => rawValue[0],
            TypeCode.DateTime => DateTime.SpecifyKind(DateTime.Parse(rawValue, null, DateTimeStyles.RoundtripKind), DateTimeKind.Utc),
            _ => rawValue
        };
    }

    private static Expression<Func<TEntity, bool>>? GetFilteringPredicate<TEntity>(Filtering filter)
    {
        if (typeof(TEntity).GetProperty(filter.Property) is not { } propertyInfo)
            return null;

        return DynamicExpressions.DynamicExpressions.GetPredicate<TEntity>(filter.Property, Convert(filter.Type), ConvertValue(propertyInfo.PropertyType, filter.Value));
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
}