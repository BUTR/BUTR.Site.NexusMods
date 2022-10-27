using System;
using System.Collections.Generic;

namespace BUTR.Site.NexusMods.Server.Extensions
{
    public static class ListExtensions
    {
        public static int BinarySearch<T, TKey>(this IList<T> instance, TKey itemKey, Func<T, TKey> keySelector) where TKey : IComparable<TKey>, IComparable
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            int start = 0;
            int end = instance.Count - 1;

            while (start <= end)
            {
                int m = (start + end) / 2;
                TKey key = keySelector(instance[m]);
                int result = key.CompareTo(itemKey);
                if (result == 0)
                    return m;
                if (result < 0)
                    start = m + 1;
                else
                    end = m - 1;
            }

            return ~start;
        }
    }
}