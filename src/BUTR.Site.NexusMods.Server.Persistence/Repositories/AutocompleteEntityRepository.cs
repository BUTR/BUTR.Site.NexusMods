using BUTR.Site.NexusMods.Server.Models.Database;

using System;
using System.Collections.Generic;
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