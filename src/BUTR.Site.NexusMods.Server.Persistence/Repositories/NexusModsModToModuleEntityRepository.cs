using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;

using System;
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
    public required LinkedByStaffNexusModsModModel[] Mods { get; init; }
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



public sealed record LinkedByStaffSteamWorkshopModModel
{
    public SteamWorkshopModId SteamWorkshopModId { get; init; }
    public DateTimeOffset LastCheckedDate { get; init; }
}

public sealed record LinkedByStaffModuleSteamWorkshopModsModel
{
    public required ModuleId ModuleId { get; init; }
    public required LinkedByStaffSteamWorkshopModModel[] Mods { get; init; }
}

public sealed record LinkedByExposureSteamWorkshopModModelsModel
{
    public required SteamWorkshopModId SteamWorkshopModId { get; init; }
    public required LinkedByExposureModuleModel[] Modules { get; init; }
}

public interface ISteamWorkshopModToModuleEntityRepositoryRead : IRepositoryRead<SteamWorkshopModToModuleEntity>
{
    Task<Paging<LinkedByStaffModuleSteamWorkshopModsModel>> GetByStaffPaginatedAsync(PaginatedQuery query, CancellationToken ct);

    Task<Paging<LinkedByExposureSteamWorkshopModModelsModel>> GetExposedPaginatedAsync(PaginatedQuery query, CancellationToken ct);
}
public interface ISteamWorkshopModToModuleEntityRepositoryWrite : IRepositoryWrite<SteamWorkshopModToModuleEntity>, ISteamWorkshopModToModuleEntityRepositoryRead;