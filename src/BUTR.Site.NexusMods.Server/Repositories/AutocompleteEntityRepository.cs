using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Jobs;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Repositories;

public interface IAutocompleteEntityRepositoryRead : IRepositoryRead<AutocompleteEntity>
{
    Task<IList<string>> AutocompleteStartsWithAsync<TEntity, TParameter>(Expression<Func<TEntity, TParameter>> property, TParameter value, CancellationToken ct)
        where TEntity : class, IEntity;
}
public interface IAutocompleteEntityRepositoryWrite : IRepositoryWrite<AutocompleteEntity>, IAutocompleteEntityRepositoryRead;

[ScopedService<IAutocompleteEntityRepositoryWrite, IAutocompleteEntityRepositoryRead>]
internal class AutocompleteEntityRepository : Repository<AutocompleteEntity>, IAutocompleteEntityRepositoryWrite
{
    protected override IQueryable<AutocompleteEntity> InternalQuery => base.InternalQuery;

    public AutocompleteEntityRepository(IAppDbContextProvider appDbContextProvider) : base(appDbContextProvider.Get()) { }

    public async Task<IList<string>> AutocompleteStartsWithAsync<TEntity, TParameter>(Expression<Func<TEntity, TParameter>> property, TParameter value, CancellationToken ct)
        where TEntity : class, IEntity
    {
        var key = AutocompleteProcessorProcessorJob.GenerateName(property);
        return await _dbContext.Autocompletes
            .Where(x => x.Type == key)
            .Where(x => EF.Functions.ILike(x.Value, $"%{value}%"))
            .Select(x => x.Value)
            .OrderBy(x => x)
            .ToListAsync(ct);
    }
}