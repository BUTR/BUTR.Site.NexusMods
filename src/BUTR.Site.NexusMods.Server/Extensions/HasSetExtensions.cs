using System.Collections.Generic;

namespace BUTR.Site.NexusMods.Server.Extensions;

/// <summary>
/// Provides extension methods for <see cref="HashSet{T}"/> objects.
/// </summary>
public static class HasSetExtensions
{
    /// <summary>
    /// Adds a range of elements to a hash set.
    /// </summary>
    /// <typeparam name="T">The type of elements in the hash set and enumerable.</typeparam>
    /// <param name="hashSet">The hash set to add elements to.</param>
    /// <param name="enumerable">The enumerable whose elements to add to the hash set.</param>
    public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> enumerable)
    {
        foreach (var entry in enumerable)
            hashSet.Add(entry);
    }
}