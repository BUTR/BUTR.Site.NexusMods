using System.Collections.Generic;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record NexusModsUserMetadataEntity : IEntity
{
    public required int NexusModsUserId { get; init; }

    public required Dictionary<string, string> Metadata { get; init; }
}