using System.Collections.Generic;

namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public sealed record UserMetadataEntity : IEntity
    {
        public required int UserId { get; init; }

        public required Dictionary<string, string> Metadata { get; init; }
    }
}