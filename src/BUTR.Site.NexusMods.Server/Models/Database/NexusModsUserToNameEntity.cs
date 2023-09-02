using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record NexusModsUserToNameEntity : IEntity
{
    public required NexusModsUserEntity NexusModsUser { get; init; }

    public required string Name { get; init; }

    public override int GetHashCode() => HashCode.Combine(NexusModsUser.NexusModsUserId, Name);
}