using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record SteamWorkshopModToNameEntity : IEntityWithTenant
{
    public required TenantId TenantId { get; init; }

    public required SteamWorkshopModId SteamWorkshopModId { get; init; }
    public required SteamWorkshopModEntity SteamWorkshopMod { get; init; }

    public required string Name { get; init; }

    public override int GetHashCode() => HashCode.Combine(TenantId, SteamWorkshopModId, Name);
}