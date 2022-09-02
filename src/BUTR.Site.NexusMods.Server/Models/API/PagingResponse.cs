using System.Collections.Generic;

namespace BUTR.Site.NexusMods.Server.Models.API
{
    public sealed record PagingResponse<T> where T : class
    {
        public IAsyncEnumerable<T> Items { get; init; }
        public PagingMetadata Metadata { get; init; }
    }
}