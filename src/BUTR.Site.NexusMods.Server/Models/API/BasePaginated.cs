using BUTR.Site.NexusMods.Server.Models.Database;

using System;
using System.Collections.Generic;

namespace BUTR.Site.NexusMods.Server.Models.API;

public record BasePaginated<TSource, TResult>(Paging<TSource> Paging, Func<IAsyncEnumerable<TSource>, IAsyncEnumerable<TResult>> Transform) where TSource : class;