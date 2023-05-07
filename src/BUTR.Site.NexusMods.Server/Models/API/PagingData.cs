﻿using System.Collections.Generic;

namespace BUTR.Site.NexusMods.Server.Models.API;

public sealed record PagingData<T> where T : class
{
    public required IAsyncEnumerable<T> Items { get; init; }
    public required PagingMetadata Metadata { get; init; }
}