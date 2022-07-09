using System.Collections.Immutable;

namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public sealed record UserAllowedModsTableEntry
    {
        public int UserId { get; set; } = default!;

        public ImmutableArray<string> AllowedModIds { get; set; } = default!;
    }
}