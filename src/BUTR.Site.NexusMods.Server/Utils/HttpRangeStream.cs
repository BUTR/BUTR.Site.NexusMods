using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;

namespace BUTR.Site.NexusMods.Server.Utils
{
    public sealed record HttpRangeOptions
    {
        public int BufferSize { get; init; } = 16 * 1024;
    }

    public sealed class HttpRangeStream : Stream
    {
        public static HttpRangeStream? CreateOrDefault(Uri url, HttpClient httpClient, HttpRangeOptions options)
        {
            TryCreate(url, httpClient, options, out var httpRangeStream);
            return httpRangeStream;
        }

        public static bool TryCreate(Uri url, HttpClient httpClient, HttpRangeOptions options, [NotNullWhen(true)] out HttpRangeStream? httpRangeStream)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = url,
                Method = HttpMethod.Head
            };
            var response = httpClient.Send(request);
            var length = response.Content.Headers.ContentLength ?? 0;

            if (response.Headers.AcceptRanges.All(x => x != "bytes"))
            {
                httpRangeStream = null;
                return false;
            }

            httpRangeStream = new HttpRangeStream(url, httpClient, options.BufferSize, length);
            return true;
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length { get; }
        public override long Position { get; set; }

        private readonly Uri _url;
        private readonly HttpClient _httpClient;

        private IMemoryOwner<byte> _dowloadedDataBufferOwnner;
        private Memory<byte> _dowloadedDataBuffer;
        private long _downloadedDataBufferStartPosition = -1;

        private HttpRangeStream(Uri url, HttpClient httpClient, int bufferSize, long length)
        {
            _url = url;
            _httpClient = httpClient;
            _dowloadedDataBufferOwnner = MemoryPool<byte>.Shared.Rent(bufferSize);
            _dowloadedDataBuffer = _dowloadedDataBufferOwnner.Memory;
            Length = length;
        }

        private int Read(Span<byte> buffer, long from, long to)
        {
            var toRead = to - from;
            using var request = new HttpRequestMessage
            {
                RequestUri = _url,
                Method = HttpMethod.Get,
                Headers =
                {
                    Range = new RangeHeaderValue(from, to)
                },
            };

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            try
            {
                using var response = _httpClient.Send(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);
                using var stream = response.Content.ReadAsStream();

                var read = 0;
                while (read < toRead)
                {
                    var currRead = stream.Read(buffer.Slice(read));
                    if (currRead == 0)
                        return read;
                    read += currRead;
                }

                return read;
            }
            catch (Exception e) when (e is OperationCanceledException)
            {
                return 0;
            }
        }

        private void RefillDowloadedDataBufferFromPosition()
        {
            var min = Position;
            var max = Math.Min(Position + _dowloadedDataBuffer.Length, Length);
            var toRead = max - min;

            var read = 0;
            while (read < toRead)
            {
                var span = _dowloadedDataBuffer.Span.Slice(read);
                read += Read(span, min + read, max);
            }

            _downloadedDataBufferStartPosition = Position;
        }

        public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

        public override int Read(Span<byte> buffer)
        {
            var count = buffer.Length;
            var internalBuffer = _dowloadedDataBuffer.Span;
            while (!buffer.IsEmpty)
            {
                var downloadedDataConsumed = Position - _downloadedDataBufferStartPosition;
                var downloadedDataAvailable = _dowloadedDataBuffer.Length - downloadedDataConsumed;

                var isDownloadedDataAvailable = _downloadedDataBufferStartPosition >= 0 && downloadedDataAvailable > 0;
                var isPositionBeforeDownloadedData = Position < _downloadedDataBufferStartPosition;
                var isPositionAfterDownloadedData = Position >= _downloadedDataBufferStartPosition + _dowloadedDataBuffer.Length;
                if (!isDownloadedDataAvailable || isPositionBeforeDownloadedData || isPositionAfterDownloadedData)
                {
                    RefillDowloadedDataBufferFromPosition();
                    continue;
                }

                var downloadedDataAvailableToCopy = Math.Min((int) downloadedDataAvailable, buffer.Length);
                internalBuffer.Slice((int) downloadedDataConsumed, downloadedDataAvailableToCopy).CopyTo(buffer);
                Position += downloadedDataAvailableToCopy;
                buffer = buffer.Slice(downloadedDataAvailableToCopy);
            }
            return count;
        }

        public override long Seek(long offset, SeekOrigin origin) => origin switch
        {
            SeekOrigin.Begin => Position = offset,
            SeekOrigin.Current => Position += offset,
            SeekOrigin.End => Position = Length - offset,
            _ => Position
        };

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException("Stream doesn't support write operations!");

        public override void Flush()
        {
            _downloadedDataBufferStartPosition = -1;
            _dowloadedDataBuffer.Span.Clear();
        }

        public void SetBufferSize(int bufferSize)
        {
            _dowloadedDataBufferOwnner.Dispose();

            _dowloadedDataBufferOwnner = MemoryPool<byte>.Shared.Rent(bufferSize);
            _dowloadedDataBuffer = _dowloadedDataBufferOwnner.Memory;

            _downloadedDataBufferStartPosition = -1;
        }

        protected override void Dispose(bool disposing)
        {
            _dowloadedDataBufferOwnner.Dispose();
            _dowloadedDataBuffer = Memory<byte>.Empty;

            base.Dispose(disposing);
        }
    }
}