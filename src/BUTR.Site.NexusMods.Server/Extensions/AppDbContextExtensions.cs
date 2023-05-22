using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BUTR.Site.NexusMods.Server.Extensions;

public static class AppDbContextExtensions
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
}