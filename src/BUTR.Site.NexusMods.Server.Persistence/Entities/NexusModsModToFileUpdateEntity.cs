using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record NexusModsModToFileUpdateEntity : IEntityWithTenant
{
    public required TenantId TenantId { get; init; }

    public required NexusModsModId NexusModsModId { get; init; }
    public required NexusModsModEntity NexusModsMod { get; init; }

    public required DateTimeOffset LastCheckedDate { get; init; }

    public override int GetHashCode() => HashCode.Combine(TenantId, NexusModsModId, LastCheckedDate);
}