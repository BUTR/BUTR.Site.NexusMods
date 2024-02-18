using System;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text.Unicode;

namespace BUTR.Site.NexusMods.Server.Extensions;

/// <summary>
/// Provides extension methods for <see cref="BlobReader"/> objects.
/// </summary>
public static class BlobReaderExtensions
{
    /// <summary>
    /// Gets a span of characters representing the string in UTF-16 format from the blob reader.
    /// </summary>
    /// <param name="reader">The blob reader to get the string from.</param>
    /// <returns>A span of characters representing the string in UTF-16 format.</returns>
    public static unsafe Span<char> GetStringUTF16Span(this in BlobReader reader) => new(reader.CurrentPointer, reader.Length);

    /// <summary>
    /// Gets a span of characters representing the string in UTF-8 format from the blob reader.
    /// </summary>
    /// <param name="reader">The blob reader to get the string from.</param>
    /// <returns>A span of characters representing the string in UTF-8 format.</returns>
    [SkipLocalsInit]
    public static unsafe Span<char> GetStringUTF8Span(this in BlobReader reader)
    {
        var utf8 = new Span<byte>(reader.CurrentPointer, reader.Length);
        var data = new char[reader.Length];
        Utf8.ToUtf16(utf8, data, out _, out _);
        return data;
    }

    /// <summary>
    /// Gets a span of bytes representing the raw string from the blob reader.
    /// </summary>
    /// <param name="reader">The blob reader to get the string from.</param>
    /// <returns>A span of bytes representing the raw string.</returns>
    public static unsafe Span<byte> GetStringRawSpan(this ref BlobReader reader) => new(reader.CurrentPointer, reader.Length);
}