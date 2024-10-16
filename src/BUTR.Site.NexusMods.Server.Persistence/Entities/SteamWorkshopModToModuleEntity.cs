using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record SteamWorkshopModToModuleEntity : IEntityWithTenant
{
    public required TenantId TenantId { get; init; }

    public required SteamWorkshopModId SteamWorkshopModId { get; init; }
    public required SteamWorkshopModEntity SteamWorkshopMod { get; init; }

    public required ModuleId ModuleId { get; init; }
    public required ModuleEntity Module { get; init; }

    public required ModToModuleLinkType LinkType { get; init; }

    public required DateTimeOffset LastUpdateDate { get; init; }

    public override int GetHashCode() => HashCode.Combine(TenantId, SteamWorkshopModId, ModuleId, LinkType, LastUpdateDate);
}