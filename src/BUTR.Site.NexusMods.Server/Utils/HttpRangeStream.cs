using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Utils
{
    public class RangeDownloadedEventArgs : EventArgs
    {
        public long Offset { get; set; }

        public long Length { get; set; }
    }

    public sealed class HttpRangeStream : Stream
    {
        public event EventHandler<RangeDownloadedEventArgs> RangeDownloaded;

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length { get; }
        public override long Position { get; set; }

        private readonly Uri _url;
        private readonly HttpClient _httpClient;

        private readonly byte[] _buffer = new byte[1024 * 512];
        private long _bufferStartPosition = -1;

        public HttpRangeStream(Uri url, HttpClient httpClient)
        {
            _url = url;
            _httpClient = httpClient;

            var request = new HttpRequestMessage
            {
                RequestUri = _url,
                Method = HttpMethod.Head
            };
            var response = _httpClient.Send(request);
            Length = response.Content.Headers.ContentLength ?? 0;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_bufferStartPosition == -1 || (_bufferStartPosition >= Position + count && _bufferStartPosition >= Position + count))
            {
                var request = new HttpRequestMessage
                {
                    RequestUri = _url,
                    Method = HttpMethod.Get,
                    Headers =
                    {
                        Range = new RangeHeaderValue(Position, Position + _buffer.Length)
                    }
                };
                var response = _httpClient.Send(request);
                var stream = response.Content.ReadAsStream();
                var read = stream.Read(_buffer, 0, _buffer.Length);
                _bufferStartPosition = Position;
            }

            _buffer.AsSpan((int) (Position - _bufferStartPosition), count).CopyTo(buffer.AsSpan(offset, count));
            Position += count;

            return count;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (_bufferStartPosition == -1 || (_bufferStartPosition >= Position + count && _bufferStartPosition >= Position + count))
            {
                var request = new HttpRequestMessage
                {
                    RequestUri = _url,
                    Method = HttpMethod.Get,
                    Headers =
                    {
                        Range = new RangeHeaderValue(Position, Position + _buffer.Length)
                    }
                };
                var response = await _httpClient.SendAsync(request, cancellationToken);
                var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                var read = await stream.ReadAsync(_buffer, 0, _buffer.Length, cancellationToken);
                _bufferStartPosition = Position;
                RangeDownloaded?.Invoke(this, new RangeDownloadedEventArgs { Offset = Position, Length = _buffer.Length });
            }

            _buffer.AsSpan((int) (Position - _bufferStartPosition), count).CopyTo(buffer.AsSpan(offset, count));
            Position += count;

            return count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    if (_bufferStartPosition >= offset || _bufferStartPosition + _buffer.Length <= offset)
                        _bufferStartPosition = -1;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    _bufferStartPosition = -1;
                    break;
                case SeekOrigin.End:
                    Position = Length - offset;
                    _bufferStartPosition = -1;
                    break;
            }

            return Position;
        }

        public override void SetLength(long value) { }

        public override void Write(byte[] buffer, int offset, int count) { }

        public override void Flush() { }

    }
}