using System.Collections.Immutable;

namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public sealed record NexusModsModTableEntry
    {
        public string Name { get; set; } = default!;

        public int ModId { get; set; } = default!;

        public ImmutableArray<int> UserIds { get; set; } = default!;
    }
}