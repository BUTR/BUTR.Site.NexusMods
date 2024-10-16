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

[ScopedService<INexusModsModToModuleEntityRepositoryWrite, INexusModsModToModuleEntityRepositoryRead>]
internal class NexusModsModToModuleEntityRepository : Repository<NexusModsModToModuleEntity>, INexusModsModToModuleEntityRepositoryWrite
{
    protected override IQueryable<NexusModsModToModuleEntity> InternalQuery => base.InternalQuery
        .Include(x => x.NexusModsMod).ThenInclude(x => x.Name)
        .Include(x => x.Module);

    public NexusModsModToModuleEntityRepository(IAppDbContextProvider appDbContextProvider) : base(appDbContextProvider.Get()) { }

    public async Task<Paging<LinkedByStaffModuleNexusModsModsModel>> GetByStaffPaginatedAsync(PaginatedQuery query, CancellationToken ct) => await _dbContext.NexusModsModModules
        .Where(x => x.LinkType == ModToModuleLinkType.ByStaff)
        .GroupBy(x => new { x.ModuleId })
        .Select(x => new LinkedByStaffModuleNexusModsModsModel
        {
            ModuleId = x.Key.ModuleId,
            NexusModsMods = x.Select(y => new LinkedByStaffNexusModsModModel
            {
                NexusModsModId = y.NexusModsModId,
                LastCheckedDate = y.LastUpdateDate.ToUniversalTime(),
            }).ToArray()
        })
        .PaginatedAsync(query, 100, new() { Property = nameof(LinkedByStaffModuleNexusModsModsModel.ModuleId), Type = SortingType.Ascending }, ct);

    public async Task<Paging<LinkedByExposureNexusModsModModelsModel>> GetExposedPaginatedAsync(PaginatedQuery query, CancellationToken ct) => await _dbContext.NexusModsModModules
        .Where(x => x.LinkType == ModToModuleLinkType.ByUnverifiedFileExposure)
        .GroupBy(x => new { x.NexusModsModId })
        .Select(x => new LinkedByExposureNexusModsModModelsModel
        {
            NexusModsModId = x.Key.NexusModsModId,
            Modules = x.Select(y => new LinkedByExposureModuleModel
            {
                ModuleId = y.ModuleId,
                LastCheckedDate = y.LastUpdateDate.ToUniversalTime(),
            }).ToArray()
        })
        .PaginatedAsync(query, 100, new() { Property = nameof(LinkedByExposureNexusModsModModelsModel.NexusModsModId), Type = SortingType.Ascending }, ct);
}