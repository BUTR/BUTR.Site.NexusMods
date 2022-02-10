using System.Collections.Generic;
using System.Linq;

namespace BUTR.Site.NexusMods.Shared.Models.API
{
    public record PagingResponse<T> where T : class
    {
        public IEnumerable<T> Items { get; init; } = Enumerable.Empty<T>();
        public PagingMetadata Metadata { get; init; } = new();
    }
}