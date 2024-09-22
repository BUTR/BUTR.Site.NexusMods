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
}