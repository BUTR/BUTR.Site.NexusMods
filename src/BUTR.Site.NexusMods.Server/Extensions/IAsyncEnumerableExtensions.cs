using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Extensions;

public static class IAsyncEnumerableExtensions
{
    private static ConfiguredCancelableAsyncEnumerable<T>.Enumerator GetConfiguredAsyncEnumerator<T>(this IAsyncEnumerable<T> enumerable, CancellationToken cancellationToken, bool continueOnCapturedContext)
    {
        return enumerable.ConfigureAwait(continueOnCapturedContext).WithCancellation(cancellationToken).GetAsyncEnumerator();
    }

    public static async Task<ImmutableArray<TSource>> ToImmutableArrayAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken ct = default)
    {
        var builder = ImmutableArray.CreateBuilder<TSource>();
        await foreach (var element in source.WithCancellation(ct))
            builder.Add(element);
        return builder.ToImmutable();
    }

    public static async IAsyncEnumerable<IReadOnlyCollection<TSource>> ChunkAsync<TSource>(this IAsyncEnumerable<TSource> source, int size, [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (size < 1)
            throw new ArgumentOutOfRangeException(nameof(size));

        await using var e = source.GetConfiguredAsyncEnumerator(ct, false);
        var buffer = new List<TSource>(size);
        while (await e.MoveNextAsync())
        {
            buffer.Add(e.Current);
            if (buffer.Count >= size)
            {
                yield return buffer;
                buffer = new List<TSource>(size);
            }
        }

        if (buffer.Count > 0)
        {
            yield return buffer;
        }
    }
}