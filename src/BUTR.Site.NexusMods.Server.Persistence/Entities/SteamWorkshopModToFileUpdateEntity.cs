using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record SteamWorkshopModToFileUpdateEntity : IEntityWithTenant
{
    public required TenantId TenantId { get; init; }

    public required SteamWorkshopModId SteamWorkshopModId { get; init; }
    public required SteamWorkshopModEntity SteamWorkshopMod { get; init; }

    public required DateTimeOffset LastCheckedDate { get; init; }

    public override int GetHashCode() => HashCode.Combine(TenantId, SteamWorkshopModId, LastCheckedDate);
}