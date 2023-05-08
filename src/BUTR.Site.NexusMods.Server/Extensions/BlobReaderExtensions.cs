using System;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text.Unicode;

namespace BUTR.Site.NexusMods.Server.Extensions;

public static class BlobReaderExtensions
{
    public static unsafe Span<char> GetStringUTF16Span(this in BlobReader reader) => new(reader.CurrentPointer, reader.Length);
    [SkipLocalsInit]
    public static unsafe Span<char> GetStringUTF8Span(this in BlobReader reader)
    {
        var utf8 = new Span<byte>(reader.CurrentPointer, reader.Length);
        var data = new char[reader.Length];
        Utf8.ToUtf16(utf8, data, out _, out _);
        return data;
    }

    public static unsafe Span<byte> GetStringRawSpan(this ref BlobReader reader) => new(reader.CurrentPointer, reader.Length);
}