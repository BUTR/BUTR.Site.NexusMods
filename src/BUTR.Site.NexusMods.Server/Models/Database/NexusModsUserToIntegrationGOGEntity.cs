using System;
using System.Collections.Generic;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record NexusModsUserToIntegrationGOGEntity : IEntity
{
    public required NexusModsUserId NexusModsUserId { get; init; }
    public required NexusModsUserEntity NexusModsUser { get; init; }
    public IntegrationGOGTokensEntity? ToTokens { get; init; }

    public required string GOGUserId { get; init; }
    public ICollection<IntegrationGOGToOwnedTenantEntity> ToOwnedTenants { get; init; } = new List<IntegrationGOGToOwnedTenantEntity>();

    public override int GetHashCode() => HashCode.Combine(NexusModsUserId, GOGUserId);
}