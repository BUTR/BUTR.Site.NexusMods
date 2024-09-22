using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record IntegrationGOGToOwnedTenantEntity : IEntity
{
    public required string GOGUserId { get; init; }
    public required TenantId OwnedTenant { get; init; }

    public override int GetHashCode() => HashCode.Combine(GOGUserId, OwnedTenant);
}