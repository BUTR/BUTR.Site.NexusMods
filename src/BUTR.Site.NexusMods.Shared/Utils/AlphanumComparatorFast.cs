using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BUTR.Site.NexusMods.Shared.Utils;

public sealed class AlphanumComparatorFast : IComparer<string?>, IComparer
{
    /// <inheritdoc/>
    public int Compare(object? x, object? y) => Compare(x as string, y as string);

    /// <inheritdoc/>
    public int Compare(string? s1, string? s2)
    {
        if (s1 is null && s2 is null) return 0;
        if (s1 is null) return -1;
        if (s2 is null) return 1;

        var len1 = s1.Length;
        var len2 = s2.Length;
        if (len1 == 0 && len2 == 0) return 0;
        if (len1 == 0) return -1;
        if (len2 == 0) return 1;

        var marker1 = 0;
        var marker2 = 0;
        while (marker1 < len1 || marker2 < len2)
        {
            if (marker1 >= len1) return -1;
            if (marker2 >= len2) return 1;
            var ch1 = s1[marker1];
            var ch2 = s2[marker2];

            var chunk1 = new StringBuilder();
            var chunk2 = new StringBuilder();

            while (marker1 < len1 && (chunk1.Length == 0 || InChunk(ch1, chunk1[0])))
            {
                chunk1.Append(ch1);
                marker1++;

                if (marker1 < len1)
                    ch1 = s1[marker1];
            }

            while (marker2 < len2 && (chunk2.Length == 0 || InChunk(ch2, chunk2[0])))
            {
                chunk2.Append(ch2);
                marker2++;

                if (marker2 < len2)
                    ch2 = s2[marker2];
            }

            // If both chunks contain numeric characters, sort them numerically
            if (char.IsDigit(chunk1[0]) && char.IsDigit(chunk2[0]))
            {
                var numericChunk1 = Convert.ToInt32(chunk1.ToString(), CultureInfo.InvariantCulture);
                var numericChunk2 = Convert.ToInt32(chunk2.ToString(), CultureInfo.InvariantCulture);

                if (numericChunk1 < numericChunk2) return -1;
                if (numericChunk1 > numericChunk2) return 1;
            }
            else
            {
                var result = string.CompareOrdinal(chunk1.ToString(), chunk2.ToString());
                // Normalize for cases when the code can't handle 2 or -2 and futher
                if (result >= 1) return 1;
                if (result <= -1) return -1;
            }
        }

        return 0;
    }

    private static bool InChunk(char ch, char otherCh)
    {
        var type = ChunkType.Alphanumeric;

        if (char.IsDigit(otherCh))
            type = ChunkType.Numeric;

        return (type != ChunkType.Alphanumeric || !char.IsDigit(ch)) && (type != ChunkType.Numeric || char.IsDigit(ch));
    }

    private enum ChunkType
    {
        Alphanumeric,
        Numeric
    }
}