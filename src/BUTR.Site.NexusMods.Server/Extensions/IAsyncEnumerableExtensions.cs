using JetBrains.Annotations;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IAsyncEnumerable{T}"/> objects.
/// </summary>
public static class IAsyncEnumerableExtensions
{
    public static int IndexOf<T>(this IEnumerable<T> source, Predicate<T> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        var index = 0;
        foreach (var item in source)
        {
            if (predicate(item))
                return index;

            index += 1;
        }

        return -1;
    }

    /// <summary>
    /// Configures an async enumerator with a cancellation token and a flag indicating whether to continue on a captured context.
    /// </summary>
    /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
    /// <param name="enumerable">The async enumerable to configure.</param>
    /// <param name="continueOnCapturedContext">A flag indicating whether to continue on a captured context.</param>
    /// <param name="ct">The cancellation token to use.</param>
    /// <returns>A configured async enumerator.</returns>
    [MustDisposeResource]
    private static ConfiguredCancelableAsyncEnumerable<T>.Enumerator GetConfiguredAsyncEnumerator<T>(this IAsyncEnumerable<T> enumerable, bool continueOnCapturedContext, CancellationToken ct)
    {
        return enumerable.ConfigureAwait(continueOnCapturedContext).WithCancellation(ct).GetAsyncEnumerator();
    }

    /// <summary>
    /// Converts an async enumerable to an immutable array.
    /// </summary>
    /// <typeparam name="TSource">The type of elements in the enumerable.</typeparam>
    /// <param name="source">The async enumerable to convert.</param>
    /// <param name="ct">The cancellation token to use.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the immutable array.</returns>
    public static async Task<ImmutableArray<TSource>> ToImmutableArrayAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken ct = default)
    {
        var builder = ImmutableArray.CreateBuilder<TSource>();
        await foreach (var element in source.WithCancellation(ct))
            builder.Add(element);
        return builder.ToImmutable();
    }

    /// <summary>
    /// Splits an async enumerable into chunks of a specified size.
    /// </summary>
    /// <typeparam name="TSource">The type of elements in the enumerable.</typeparam>
    /// <param name="source">The async enumerable to chunk.</param>
    /// <param name="size">The size of the chunks.</param>
    /// <param name="ct">The cancellation token to use.</param>
    /// <returns>An async enumerable that contains the chunks.</returns>
    public static async IAsyncEnumerable<IReadOnlyCollection<TSource>> ChunkAsync<TSource>(this IAsyncEnumerable<TSource> source, int size, [EnumeratorCancellation] CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfLessThan(size, 1);

        await using var e = source.GetConfiguredAsyncEnumerator(false, ct);
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