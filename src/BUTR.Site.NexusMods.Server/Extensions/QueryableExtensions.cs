using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models;
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

namespace BUTR.Site.NexusMods.Server.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<string> AutocompleteStartsWith<TEntity, TProperty>(this AppDbContext dbContext, Expression<Func<TEntity, TProperty>> property, string value)
        {
            if (property is not LambdaExpression { Body: MemberExpression { Member: PropertyInfo propertyInfo } }) return Enumerable.Empty<string>().AsQueryable();
            if (!property.Type.IsGenericType || property.Type.GenericTypeArguments.Length != 2) Enumerable.Empty<string>().AsQueryable();

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
                    TotalPages = (uint) Math.Floor((double) count / (double) pageSize)
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

            if (type.IsGenericType && type.GenericTypeArguments.Length == 1)
                type = type.GenericTypeArguments[0];

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.String:
                    return rawValue;
                case TypeCode.Byte:
                    return byte.Parse(rawValue);
                case TypeCode.SByte:
                    return sbyte.Parse(rawValue);
                case TypeCode.UInt16:
                    return ushort.Parse(rawValue);
                case TypeCode.UInt32:
                    return uint.Parse(rawValue);
                case TypeCode.UInt64:
                    return ulong.Parse(rawValue);
                case TypeCode.Int16:
                    return short.Parse(rawValue);
                case TypeCode.Int32:
                    return int.Parse(rawValue);
                case TypeCode.Int64:
                    return long.Parse(rawValue);
                case TypeCode.Decimal:
                    return decimal.Parse(rawValue);
                case TypeCode.Double:
                    return double.Parse(rawValue);
                case TypeCode.Single:
                    return float.Parse(rawValue);
                case TypeCode.Boolean:
                    return bool.Parse(rawValue);
                case TypeCode.Char:
                    return rawValue[0];
                case TypeCode.DateTime:
                    return DateTime.SpecifyKind(DateTime.Parse(rawValue, null, DateTimeStyles.RoundtripKind), DateTimeKind.Utc);
            }

            return rawValue;
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
}