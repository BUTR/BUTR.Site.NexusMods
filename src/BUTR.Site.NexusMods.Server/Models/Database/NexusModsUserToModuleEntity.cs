using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record NexusModsUserToModuleEntity : IEntityWithTenant
{
    public required TenantId TenantId { get; init; }

    public required NexusModsUserEntity NexusModsUser { get; init; }

    public required ModuleEntity Module { get; init; }

    public required NexusModsUserToModuleLinkType LinkType { get; init; }

    public override int GetHashCode() => HashCode.Combine(TenantId, NexusModsUser.NexusModsUserId, Module.ModuleId, LinkType);
}