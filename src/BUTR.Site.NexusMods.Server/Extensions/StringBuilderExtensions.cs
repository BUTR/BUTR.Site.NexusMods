using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Unicode;

namespace BUTR.Site.NexusMods.Server.Extensions;

public static class StringBuilderExtensions
{
    public static int IndexOf(this StringBuilder sb, ReadOnlySpan<char> value, int startIndex)
    {
        var length = value.Length;
        var maxSearchLength = (sb.Length - length) + 1;

        for (var i = startIndex; i < maxSearchLength; ++i)
        {
            if (sb[i] != value[0]) continue;

            var index = 1;
            while (index < length && (sb[i + index] == value[index]))
                ++index;

            if (index == length)
                return i;
        }

        return -1;
    }

    public static int IndexOf(this StringBuilder sb, char c)
    {
        var pos = 0;
        foreach (var chunk in sb.GetChunks())
        {
            var span = chunk.Span;
            for (var i = 0; i < span.Length; i++)
            {
                if (span[i] == c)
                {
                    return pos + i;
                }
            }

            pos += span.Length;
        }

        return -1;
    }

    [SkipLocalsInit]
    public static unsafe StringBuilder Append(this StringBuilder sb, Span<byte> rawUtf8)
    {
        const int maxLength = 1024 * 512;
        Span<char> utf16 = stackalloc char[maxLength];
        if (rawUtf8.Length < maxLength) // Fast path
        {
            var toTake = Math.Min(maxLength, rawUtf8.Length);
            var utf8Buffer = rawUtf8;
            var utf16Buffer = utf16.Slice(0, toTake);
            Utf8.ToUtf16(utf8Buffer, utf16Buffer, out _, out _);
            sb.Append(utf16Buffer);
            return sb;
        }

        var offset = 0;
        for (var i = 0; i < ((rawUtf8.Length / maxLength) + 1); i++)
        {
            var toTake = Math.Min(maxLength, rawUtf8.Length - offset);
            var utf8Buffer = rawUtf8.Slice(i * maxLength, toTake);
            var utf16Buffer = utf16.Slice(0, toTake);
            Utf8.ToUtf16(utf8Buffer, utf16Buffer, out _, out _);
            sb.Append(utf16Buffer);
            offset += toTake;
        }
        return sb;
    }
}