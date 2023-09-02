using BUTR.Site.NexusMods.Shared;

using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record NexusModsModToFileUpdateEntity : IEntityWithTenant
{
    public required Tenant TenantId { get; init; }

    public required NexusModsModEntity NexusModsMod { get; init; }

    public required DateTime LastCheckedDate { get; init; }

    public override int GetHashCode() => HashCode.Combine(TenantId, NexusModsMod.NexusModsModId, LastCheckedDate);
}