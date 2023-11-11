using System.Collections.Generic;

namespace BUTR.Site.NexusMods.Server.Extensions;

public static class HasSetExtensions
{
    public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> enumerable)
    {
        foreach (var entry in enumerable)
            hashSet.Add(entry);
    }
}