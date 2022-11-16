using BUTR.Site.NexusMods.Server.Models.API;

using System.Collections.Immutable;

namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public sealed record Paging<T> where T : class
    {
        public required ImmutableArray<T> Items { get; init; }
        public required PagingMetadata Metadata { get; init; }
    }
}