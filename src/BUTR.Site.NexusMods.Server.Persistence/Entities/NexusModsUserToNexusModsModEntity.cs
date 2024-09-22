using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public record NexusModsUserToNexusModsModEntity : IEntityWithTenant
{
    public required TenantId TenantId { get; init; }

    public required NexusModsUserId NexusModsUserId { get; init; }
    public required NexusModsUserEntity NexusModsUser { get; init; }

    public required NexusModsModId NexusModsModId { get; init; }
    public required NexusModsModEntity NexusModsMod { get; init; }

    public required NexusModsUserToNexusModsModLinkType LinkType { get; init; }

    public override int GetHashCode() => HashCode.Combine(TenantId, NexusModsUserId, NexusModsModId, LinkType);
}