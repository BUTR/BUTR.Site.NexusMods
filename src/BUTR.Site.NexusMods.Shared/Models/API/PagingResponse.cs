using System.Collections.Generic;
using System.Linq;

namespace BUTR.Site.NexusMods.Shared.Models.API
{
    public record PagingResponse<T> where T : class
    {
        public IAsyncEnumerable<T> Items { get; init; } = AsyncEnumerable.Empty<T>();
        public PagingMetadata Metadata { get; init; } = new();
    }
}