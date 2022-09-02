using System.Collections.Generic;

namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public sealed record UserMetadataEntity : IEntity
    {
        public int UserId { get; set; } = default!;

        public Dictionary<string, string> Metadata { get; set; } = default!;
    }
}