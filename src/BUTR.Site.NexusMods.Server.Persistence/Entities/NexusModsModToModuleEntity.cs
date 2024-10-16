using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record NexusModsModToModuleEntity : IEntityWithTenant
{
    public required TenantId TenantId { get; init; }

    public required NexusModsModId NexusModsModId { get; init; }
    public required NexusModsModEntity NexusModsMod { get; init; }

    public required ModuleId ModuleId { get; init; }
    public required ModuleEntity Module { get; init; }

    public required ModToModuleLinkType LinkType { get; init; }

    public required DateTimeOffset LastUpdateDate { get; init; }

    public override int GetHashCode() => HashCode.Combine(TenantId, NexusModsModId, ModuleId, LinkType, LastUpdateDate);
}