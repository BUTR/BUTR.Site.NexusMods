using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace BUTR.Site.NexusMods.Server.Repositories;

[ScopedService<ISteamWorkshopModToModuleEntityRepositoryWrite, ISteamWorkshopModToModuleEntityRepositoryRead>]
internal class SteamWorkshopModToModuleEntityRepository : Repository<SteamWorkshopModToModuleEntity>, ISteamWorkshopModToModuleEntityRepositoryWrite
{
    protected override IQueryable<SteamWorkshopModToModuleEntity> InternalQuery => base.InternalQuery
        .Include(x => x.SteamWorkshopMod).ThenInclude(x => x.Name)
        .Include(x => x.Module);

    public SteamWorkshopModToModuleEntityRepository(IAppDbContextProvider appDbContextProvider) : base(appDbContextProvider.Get()) { }

    public async Task<Paging<LinkedByStaffModuleSteamWorkshopModsModel>> GetByStaffPaginatedAsync(PaginatedQuery query, CancellationToken ct) => await _dbContext.SteamWorkshopModModules
        .Where(x => x.LinkType == ModToModuleLinkType.ByStaff)
        .GroupBy(x => new { x.ModuleId })
        .Select(x => new LinkedByStaffModuleSteamWorkshopModsModel
        {
            ModuleId = x.Key.ModuleId,
            NexusModsMods = x.Select(y => new LinkedByStaffSteamWorkshopModModel
            {
                SteamWorkshopModId = y.SteamWorkshopModId,
                LastCheckedDate = y.LastUpdateDate.ToUniversalTime(),
            }).ToArray()
        })
        .PaginatedAsync(query, 100, new() { Property = nameof(LinkedByStaffModuleSteamWorkshopModsModel.ModuleId), Type = SortingType.Ascending }, ct);

    public async Task<Paging<LinkedByExposureSteamWorkshopModModelsModel>> GetExposedPaginatedAsync(PaginatedQuery query, CancellationToken ct) => await _dbContext.SteamWorkshopModModules
        .Where(x => x.LinkType == ModToModuleLinkType.ByUnverifiedFileExposure)
        .GroupBy(x => new { x.SteamWorkshopModId })
        .Select(x => new LinkedByExposureSteamWorkshopModModelsModel
        {
            SteamWorkshopModId = x.Key.SteamWorkshopModId,
            Modules = x.Select(y => new LinkedByExposureModuleModel
            {
                ModuleId = y.ModuleId,
                LastCheckedDate = y.LastUpdateDate.ToUniversalTime(),
            }).ToArray()
        })
        .PaginatedAsync(query, 100, new() { Property = nameof(LinkedByExposureSteamWorkshopModModelsModel.SteamWorkshopModId), Type = SortingType.Ascending }, ct);
}