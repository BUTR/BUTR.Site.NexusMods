using System;
using System.Collections.Generic;

namespace BUTR.Site.NexusMods.Client.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<ElementInfo<T>> WithMetadata<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            using (var enumerator = source.GetEnumerator())
            {
                bool isFirst = true;
                bool hasNext = enumerator.MoveNext();
                int index = 0;
                while (hasNext)
                {
                    T current = enumerator.Current;
                    hasNext = enumerator.MoveNext();
                    yield return new ElementInfo<T>(index, current, isFirst, !hasNext);
                    isFirst = false;
                    index++;
                }
            }
        }

        public record ElementInfo<T>(int Index, T Value, bool IsFirst, bool IsLast);
    }
}