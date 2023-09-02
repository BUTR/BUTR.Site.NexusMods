using BUTR.Site.NexusMods.Shared;

using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record NexusModsModToNameEntity : IEntityWithTenant
{
    public required Tenant TenantId { get; init; }

    public required NexusModsModEntity NexusModsMod { get; init; }

    public required string Name { get; init; }

    public override int GetHashCode() => HashCode.Combine(TenantId, NexusModsMod.NexusModsModId, Name);
}