using BUTR.Site.NexusMods.Server.Models.API;

using System.Collections.Generic;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record Paging<T> where T : class
{
    public required long StartTime { get; init; }
    
    public required IAsyncEnumerable<T> Items { get; init; }

    public required PagingMetadata Metadata { get; init; }
}