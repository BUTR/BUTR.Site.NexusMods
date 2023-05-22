using System.IO;

namespace BUTR.Site.NexusMods.ServerClient.Utils;

partial class StreamWithNewLineEnding
{
    public override bool CanRead => _streamImplementation.CanRead;
    public override bool CanSeek => _streamImplementation.CanSeek;
    public override bool CanWrite => _streamImplementation.CanWrite;
    public override long Length => _streamImplementation.Length;
    public override long Position { get => _streamImplementation.Position; set => _streamImplementation.Position = value; }

    public override void Flush() => _streamImplementation.Flush();
    public override long Seek(long offset, SeekOrigin origin) => _streamImplementation.Seek(offset, origin);
    public override void SetLength(long value) => _streamImplementation.SetLength(value);
    public override void Write(byte[] buffer, int offset, int count) => _streamImplementation.Write(buffer, offset, count);
}