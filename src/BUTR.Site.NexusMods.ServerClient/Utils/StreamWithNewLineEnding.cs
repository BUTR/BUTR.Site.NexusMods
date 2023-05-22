﻿using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.ServerClient.Utils;

internal sealed partial class StreamWithNewLineEnding : Stream
{
    private static readonly byte[] NewLine = "\r\n"u8.ToArray();

    private readonly Stream _streamImplementation;
    private bool _endRead = false;

    public IMemoryOwner<byte>? _leftoverBytes;
    public int _leftoverBytesLength;

    public StreamWithNewLineEnding(Stream stream)
    {
        _streamImplementation = stream;
    }
    public StreamWithNewLineEnding(IMemoryOwner<byte> leftoverBytes, int leftoverBytesLength, Stream stream)
    {
        _leftoverBytes = leftoverBytes;
        _leftoverBytesLength = leftoverBytesLength;
        _streamImplementation = stream;
    }

    public override int Read(byte[] buffer, int offset, int count) => _streamImplementation.Read(buffer, offset, count);
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (_endRead) return 0;

        var length = 0;
        var newLineIdx = -1;
        if (_leftoverBytes is not null && !_leftoverBytes.Memory.IsEmpty)
        {
            length = _leftoverBytesLength;
            _leftoverBytes.Memory.CopyTo(buffer);
            ResetLeftover();
            var memory = buffer.Slice(0, length);
            if (memory.Span.IndexOfAny(NewLine) is not (var idx and not -1)) return length;
            newLineIdx = idx;
        }
        else
        {
            length = await _streamImplementation.ReadAsync(buffer, cancellationToken);
            var memory = buffer.Slice(0, length);
            if (memory.Span.IndexOfAny(NewLine) is not (var idx and not -1)) return length;
            newLineIdx = idx;
        }

        _endRead = true;
        var realLength = newLineIdx + NewLine.Length;
        var leftover = buffer.Slice(realLength, length - realLength);
        InitializeLeftover(leftover);
        return realLength;
    }

    private void InitializeLeftover(Memory<byte> leftover)
    {
        _leftoverBytes = MemoryPool<byte>.Shared.Rent(leftover.Length);
        leftover.CopyTo(_leftoverBytes.Memory);
        _leftoverBytesLength = leftover.Length;
    }

    private void ResetLeftover()
    {
        _leftoverBytes!.Dispose();
        _leftoverBytes = null;
        _leftoverBytesLength = 0;
    }
}