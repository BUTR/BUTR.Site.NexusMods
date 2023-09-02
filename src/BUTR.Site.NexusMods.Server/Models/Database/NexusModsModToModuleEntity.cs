using BUTR.Site.NexusMods.Shared;

using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record NexusModsModToModuleEntity : IEntityWithTenant
{
    public required Tenant TenantId { get; init; }

    public required NexusModsModEntity NexusModsMod { get; init; }

    public required ModuleEntity Module { get; init; }

    public required NexusModsModToModuleLinkType LinkType { get; init; }

    public required DateTime LastUpdateDate { get; init; }

    public override int GetHashCode() => HashCode.Combine(TenantId, NexusModsMod.NexusModsModId, Module.ModuleId, LinkType, LastUpdateDate);
}