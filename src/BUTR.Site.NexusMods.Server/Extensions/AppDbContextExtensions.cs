using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Jobs;

using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Linq.Expressions;

namespace BUTR.Site.NexusMods.Server.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IAppDbContextRead"/> objects.
/// </summary>
public static class AppDbContextExtensions
{
    /// <summary>
    /// Returns an IQueryable of strings where the specified property of the entity starts with the provided value.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TParameter">The type of the property.</typeparam>
    /// <param name="dbContext">The database context to query.</param>
    /// <param name="property">The property of the entity to match.</param>
    /// <param name="value">The value to match the start of the property with.</param>
    /// <returns>An IQueryable of strings where the specified property of the entity starts with the provided value.</returns>

    public static IQueryable<string> AutocompleteStartsWith<TEntity, TParameter>(this IAppDbContextRead dbContext, Expression<Func<TEntity, TParameter>> property, TParameter value)
    {
        var key = AutocompleteProcessorProcessorJob.GenerateName(property);
        return dbContext.Autocompletes
            .Where(x => x.Type == key)
            .Where(x => EF.Functions.ILike(x.Value, $"%{value}%"))
            .OrderBy(x => x.Value)
            .Select(x => x.Value);
    }

    /// <summary>
    /// Returns an IQueryable of strings where the specified property of the entity starts with the provided subValue and the property value matches the provided propertyValue.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="dbContext">The database context to query.</param>
    /// <param name="property">The property of the entity to match.</param>
    /// <param name="propertyValue">The value of the property to match.</param>
    /// <param name="subValue">The sub value to match the start of the property with.</param>
    /// <returns>An IQueryable of strings where the specified property of the entity starts with the provided subValue and the property value matches the provided propertyValue.</returns>
    public static IQueryable<string> AutocompleteGroupStartsWith<TEntity>(this IAppDbContextRead dbContext, Expression<Func<TEntity, string>> property, string propertyValue, string subValue)
    {
        var key = $"{AutocompleteProcessorProcessorJob.GenerateName(property)}.{propertyValue}";
        return dbContext.Autocompletes
            .Where(x => x.Type == key)
            .Where(x => EF.Functions.ILike(x.Value, $"%{subValue}%"))
            .OrderBy(x => x.Value)
            .Select(x => x.Value);
    }
}