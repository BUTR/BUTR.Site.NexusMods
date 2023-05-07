using System;
using System.Collections.Generic;

namespace BUTR.Site.NexusMods.Server.Extensions;

public static class IEnumerableExtensions
{
    public static int IndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        int index = 0;
        foreach (var item in source)
        {
            if (predicate(item)) return index;
            index++;
        }

        return -1;
    }
}