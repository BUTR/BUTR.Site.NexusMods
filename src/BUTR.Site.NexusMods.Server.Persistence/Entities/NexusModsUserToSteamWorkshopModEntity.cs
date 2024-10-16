using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public record NexusModsUserToSteamWorkshopModEntity : IEntityWithTenant
{
    public required TenantId TenantId { get; init; }

    public required NexusModsUserId NexusModsUserId { get; init; }
    public required NexusModsUserEntity NexusModsUser { get; init; }

    public required SteamWorkshopModId SteamWorkshopModId { get; init; }
    public required SteamWorkshopModEntity SteamWorkshopMod { get; init; }

    public required NexusModsUserToModLinkType LinkType { get; init; }

    public override int GetHashCode() => HashCode.Combine(TenantId, NexusModsUserId, SteamWorkshopModId, LinkType);
}