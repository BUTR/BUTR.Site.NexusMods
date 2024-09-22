using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record IntegrationSteamToOwnedTenantEntity : IEntity
{
    public required string SteamUserId { get; init; }
    public required TenantId OwnedTenant { get; init; }

    public override int GetHashCode() => HashCode.Combine(SteamUserId, OwnedTenant);
}