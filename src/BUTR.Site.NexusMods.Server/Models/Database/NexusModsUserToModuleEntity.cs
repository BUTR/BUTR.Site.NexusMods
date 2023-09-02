using BUTR.Site.NexusMods.Shared;

using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record NexusModsUserToModuleEntity : IEntityWithTenant
{
    public required Tenant TenantId { get; init; }

    public required NexusModsUserEntity NexusModsUser { get; init; }

    public required ModuleEntity Module { get; init; }

    public required NexusModsUserToModuleLinkType LinkType { get; init; }

    public override int GetHashCode() => HashCode.Combine(TenantId, NexusModsUser.NexusModsUserId, Module.ModuleId, LinkType);
}