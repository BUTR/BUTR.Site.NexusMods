using System;
using System.Collections.Generic;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record IntegrationSteamTokensEntity : IEntity
{
    public required NexusModsUserEntity NexusModsUser { get; init; }

    public required string SteamUserId { get; init; }
    public NexusModsUserToIntegrationSteamEntity? UserToSteam { get; init; }

    public required Dictionary<string, string> Data { get; init; }

    public override int GetHashCode() => HashCode.Combine(NexusModsUser.NexusModsUserId, SteamUserId, Data);
}