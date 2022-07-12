using BUTR.Site.NexusMods.Server.Models.API;

using System.Collections.Immutable;

namespace BUTR.Site.NexusMods.Server.Models.Database
{
    public sealed record Paging<T> where T : class
    {
        public ImmutableArray<T> Items { get; init; } = ImmutableArray<T>.Empty;
        public PagingMetadata Metadata { get; init; } = new();
    }
}