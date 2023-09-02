using BUTR.Site.NexusMods.Shared;

using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record IntegrationGOGToOwnedTenantEntity : IEntity
{
    public required string GOGUserId { get; init; }
    public required Tenant OwnedTenant { get; init; }

    public override int GetHashCode() => HashCode.Combine(GOGUserId, OwnedTenant);
}