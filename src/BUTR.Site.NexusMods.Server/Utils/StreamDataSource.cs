using AsmResolver.IO;

using System;
using System.IO;

namespace BUTR.Site.NexusMods.Server.Utils;

public class StreamDataSource : IDataSource
{
    private readonly Stream _stream;

    public StreamDataSource(Stream stream, ulong baseAddress = 0)
    {
        _stream = stream;
        BaseAddress = baseAddress;
    }

    public ulong BaseAddress { get; }

    public byte this[ulong address]
    {
        get
        {
            var temp = _stream.Position;
            _stream.Position = (long) (address + BaseAddress);
            var result = _stream.ReadByte();
            _stream.Position = temp;
            return (byte) result;
        }
    }

    public ulong Length => (ulong) _stream.Length;

    public bool IsValidAddress(ulong address) => address - BaseAddress < (ulong) _stream.Length;

    public int ReadBytes(ulong address, byte[] buffer, int index, int count)
    {
        if (!IsValidAddress(address))
            return 0;

        var relativeIndex = (int) (address - BaseAddress);
        var actualLength = (int) Math.Min(count, _stream.Length - relativeIndex);
        var temp = _stream.Position;
        _stream.Seek((long) (address + BaseAddress), SeekOrigin.Begin);
        var result = _stream.Read(buffer, index, actualLength);
        _stream.Seek(temp, SeekOrigin.Begin);
        return result;
    }
}