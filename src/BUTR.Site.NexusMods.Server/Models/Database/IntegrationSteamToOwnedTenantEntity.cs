using BUTR.Site.NexusMods.Shared;

using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record IntegrationSteamToOwnedTenantEntity : IEntity
{
    public required string SteamUserId { get; init; }
    public required Tenant OwnedTenant { get; init; }

    public override int GetHashCode() => HashCode.Combine(SteamUserId, OwnedTenant);
}