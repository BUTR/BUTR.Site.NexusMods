using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Extensions;

public static class IAsyncEnumerableExtensions
{
    public static async Task<ImmutableArray<TSource>> ToImmutableArrayAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken ct = default)
    {
        var builder = ImmutableArray.CreateBuilder<TSource>();
        await foreach (var element in source.AsAsyncEnumerable().WithCancellation(ct))
            builder.Add(element);
        return builder.ToImmutable();
    }
}