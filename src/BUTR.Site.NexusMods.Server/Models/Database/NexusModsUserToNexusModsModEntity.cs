using BUTR.Site.NexusMods.Shared;

using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public record NexusModsUserToNexusModsModEntity : IEntityWithTenant
{
    public required Tenant TenantId { get; init; }

    public required NexusModsUserEntity NexusModsUser { get; init; }

    public required NexusModsModEntity NexusModsMod { get; init; }

    public required NexusModsUserToNexusModsModLinkType LinkType { get; init; }

    public override int GetHashCode() => HashCode.Combine(TenantId, NexusModsUser.NexusModsUserId, NexusModsMod.NexusModsModId, LinkType);
}