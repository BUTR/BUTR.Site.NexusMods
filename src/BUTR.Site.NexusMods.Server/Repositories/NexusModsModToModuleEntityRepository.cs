using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Repositories;

public sealed record LinkedByStaffNexusModsModModel
{
    public NexusModsModId NexusModsModId { get; init; }
    public DateTimeOffset LastCheckedDate { get; init; }
}

public sealed record LinkedByStaffModuleNexusModsModsModel
{
    public required ModuleId ModuleId { get; init; }
    public required LinkedByStaffNexusModsModModel[] NexusModsMods { get; init; }
}

public sealed record LinkedByExposureModuleModel
{
    public required ModuleId ModuleId { get; init; }
    public required DateTimeOffset LastCheckedDate { get; init; }
}

public sealed record LinkedByExposureNexusModsModModelsModel
{
    public required NexusModsModId NexusModsModId { get; init; }
    public required LinkedByExposureModuleModel[] Modules { get; init; }
}

public interface INexusModsModToModuleEntityRepositoryRead : IRepositoryRead<NexusModsModToModuleEntity>
{
    Task<Paging<LinkedByStaffModuleNexusModsModsModel>> GetByStaffPaginatedAsync(PaginatedQuery query, CancellationToken ct);

    Task<Paging<LinkedByExposureNexusModsModModelsModel>> GetExposedPaginatedAsync(PaginatedQuery query, CancellationToken ct);
}
public interface INexusModsModToModuleEntityRepositoryWrite : IRepositoryWrite<NexusModsModToModuleEntity>, INexusModsModToModuleEntityRepositoryRead;

[ScopedService<INexusModsModToModuleEntityRepositoryWrite, INexusModsModToModuleEntityRepositoryRead>]
internal class NexusModsModToModuleEntityRepository : Repository<NexusModsModToModuleEntity>, INexusModsModToModuleEntityRepositoryWrite
{
    protected override IQueryable<NexusModsModToModuleEntity> InternalQuery => base.InternalQuery
        .Include(x => x.NexusModsMod).ThenInclude(x => x.Name)
        .Include(x => x.Module);

    public NexusModsModToModuleEntityRepository(IAppDbContextProvider appDbContextProvider) : base(appDbContextProvider.Get()) { }

    public async Task<Paging<LinkedByStaffModuleNexusModsModsModel>> GetByStaffPaginatedAsync(PaginatedQuery query, CancellationToken ct) => await _dbContext.NexusModsModModules
        .Where(x => x.LinkType == NexusModsModToModuleLinkType.ByStaff)
        .GroupBy(x => x.Module.ModuleId)
        .Select(x => new LinkedByStaffModuleNexusModsModsModel
        {
            ModuleId = x.Key,
            NexusModsMods = x.Select(y => new LinkedByStaffNexusModsModModel
            {
                NexusModsModId = y.NexusModsMod.NexusModsModId,
                LastCheckedDate = y.LastUpdateDate.ToUniversalTime(),
            }).ToArray()
        })
        .PaginatedAsync(query, 100, new() { Property = nameof(LinkedByStaffModuleNexusModsModsModel.ModuleId), Type = SortingType.Ascending }, ct);

    public async Task<Paging<LinkedByExposureNexusModsModModelsModel>> GetExposedPaginatedAsync(PaginatedQuery query, CancellationToken ct) => await _dbContext.NexusModsModModules
        .Where(x => x.LinkType == NexusModsModToModuleLinkType.ByUnverifiedFileExposure)
        .GroupBy(x => x.NexusModsMod.NexusModsModId)
        .Select(x => new LinkedByExposureNexusModsModModelsModel
        {
            NexusModsModId = x.Key,
            Modules = x.Select(y => new LinkedByExposureModuleModel
            {
                ModuleId = y.Module.ModuleId,
                LastCheckedDate = y.LastUpdateDate.ToUniversalTime(),
            }).ToArray()
        })
        .PaginatedAsync(query, 100, new() { Property = nameof(LinkedByExposureNexusModsModModelsModel.NexusModsModId), Type = SortingType.Ascending }, ct);
}