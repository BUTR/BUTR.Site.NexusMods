using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Jobs;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using EFCore.BulkExtensions;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Repositories;

[ScopedService<INexusModsArticleEntityRepositoryWrite, INexusModsArticleEntityRepositoryRead>]
internal class NexusModsArticleEntityRepository : Repository<NexusModsArticleEntity>, INexusModsArticleEntityRepositoryWrite
{
    private readonly ITenantContextAccessor _tenantContextAccessor;

    protected override IQueryable<NexusModsArticleEntity> InternalQuery => base.InternalQuery;

    public NexusModsArticleEntityRepository(IAppDbContextProvider appDbContextProvider, ITenantContextAccessor tenantContextAccessor) : base(appDbContextProvider.Get())
    {
        _tenantContextAccessor = tenantContextAccessor;
    }

    public async Task<IList<string>> GetAllModuleIdsAsync(string authorName, CancellationToken ct) => await _dbContext.NexusModsArticles
        .Select(x => x.NexusModsUser)
        .Select(x => x.Name!)
        .Where(x => EF.Functions.ILike(x.Name.Value, $"{authorName}%"))
        .Select(x => x.Name.Value)
        .Distinct()
        .ToListAsync(ct);

    public async Task GenerateAutoCompleteForAuthorNameAsync(CancellationToken ct)
    {
        var tenant = _tenantContextAccessor.Current;
        var key = AutocompleteProcessorProcessorJob.GenerateName<NexusModsArticleEntity, NexusModsUserName>(x => x.NexusModsUser.Name!.Name);

        await _dbContext.Autocompletes.Where(x => x.Type == key).ExecuteDeleteAsync(ct);

        var data = await _dbContext.NexusModsArticles.Select(y => y.NexusModsUser.Name!.Name.Value).Distinct().Select(x => new AutocompleteEntity
        {
            AutocompleteId = default,
            TenantId = tenant,
            Type = key,
            Value = x,
        }).ToListAsync(ct);
        await _dbContext.BulkInsertOrUpdateAsync(data, cancellationToken: ct);
    }
}