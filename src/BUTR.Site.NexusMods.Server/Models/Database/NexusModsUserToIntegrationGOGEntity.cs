using System;
using System.Collections.Generic;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record NexusModsUserToIntegrationGOGEntity : IEntity
{
    public required NexusModsUserEntity NexusModsUser { get; init; }
    public IntegrationGOGTokensEntity? ToTokens { get; init; }

    public required string GOGUserId { get; init; }
    public ICollection<IntegrationGOGToOwnedTenantEntity> ToOwnedTenants { get; init; }

    public override int GetHashCode() => HashCode.Combine(NexusModsUser.NexusModsUserId, GOGUserId);
}