using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Jobs;

using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Linq.Expressions;

namespace BUTR.Site.NexusMods.Server.Extensions;

public static class AppDbContextExtensions
{
    public static IQueryable<string> AutocompleteStartsWith<TEntity, TParameter>(this IAppDbContextRead dbContext, Expression<Func<TEntity, TParameter>> property, TParameter value)
    {
        var key = AutocompleteProcessorProcessorJob.GenerateName(property);
        return dbContext.Autocompletes
            .Where(x => x.Type == key)
            .Where(x => EF.Functions.ILike(x.Value, $"%{value}%"))
            .OrderBy(x => x.Value)
            .Select(x => x.Value);
    }

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