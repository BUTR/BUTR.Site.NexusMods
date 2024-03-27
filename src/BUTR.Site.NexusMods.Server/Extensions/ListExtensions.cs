using System;
using System.Collections.Generic;

namespace BUTR.Site.NexusMods.Server.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IList{T}"/> objects.
/// </summary>
public static class ListExtensions
{
    /// <summary>
    /// Performs a binary search on the specified list, using a key selector function to compare elements.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <typeparam name="TKey">The type of the key to search for.</typeparam>
    /// <param name="instance">The list to search.</param>
    /// <param name="itemKey">The key to search for.</param>
    /// <param name="keySelector">A function to extract a key from an element.</param>
    /// <returns>The zero-based index of item in the sorted List{T}, if item is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than item or, if there is no larger element, the bitwise complement of Count.</returns>

    public static int BinarySearch<T, TKey>(this IList<T> instance, TKey itemKey, Func<T, TKey> keySelector) where TKey : IComparable<TKey>, IComparable
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(keySelector);

        int start = 0, end = instance.Count - 1;

        while (start <= end)
        {
            var mid = (start + end) / 2;
            var result = keySelector(instance[mid]).CompareTo(itemKey);

            if (result == 0)
                return mid;
            if (result < 0)
                start = mid + 1;
            else
                end = mid - 1;
        }

        return ~start;
    }
}