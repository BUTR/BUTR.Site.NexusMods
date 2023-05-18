using System;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text.Unicode;

namespace BUTR.Site.NexusMods.Shared.Extensions;

public static class MetadataReaderExtensions
{
    public static unsafe Span<char> GetStringUTF16Span(this MetadataReader metadataReader, StringHandle stringHandle)
    {
        var reader = metadataReader.GetBlobReader(stringHandle);
        return new(reader.CurrentPointer, reader.Length);
    }
    
    [SkipLocalsInit]
    public static unsafe Span<char> GetStringUTF8Span(this MetadataReader metadataReader, StringHandle stringHandle)
    {
        var reader = metadataReader.GetBlobReader(stringHandle);
        var utf8 = new Span<byte>(reader.CurrentPointer, reader.Length);
        var data = new char[reader.Length];
        Utf8.ToUtf16(utf8, data, out _, out _);
        return data;
    }

    public static unsafe Span<byte> GetStringRawSpan(this MetadataReader metadataReader, StringHandle stringHandle)
    {
        var reader = metadataReader.GetBlobReader(stringHandle);
        return new(reader.CurrentPointer, reader.Length);
    }
    
    public static unsafe Span<char> GetStringUTF16Span(this MetadataReader metadataReader, BlobHandle blobHandle)
    {
        var reader = metadataReader.GetBlobReader(blobHandle);
        return new(reader.CurrentPointer, reader.Length);
    }
    
    [SkipLocalsInit]
    public static unsafe Span<char> GetStringUTF8Span(this MetadataReader metadataReader, BlobHandle blobHandle)
    {
        var reader = metadataReader.GetBlobReader(blobHandle);
        var utf8 = new Span<byte>(reader.CurrentPointer, reader.Length);
        var data = new char[reader.Length];
        Utf8.ToUtf16(utf8, data, out _, out _);
        return data;
    }

    public static unsafe Span<byte> GetStringRawSpan(this MetadataReader metadataReader, BlobHandle blobHandle)
    {
        var reader = metadataReader.GetBlobReader(blobHandle);
        return new(reader.CurrentPointer, reader.Length);
    }
}