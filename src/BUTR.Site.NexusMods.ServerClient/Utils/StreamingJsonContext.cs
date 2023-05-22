using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.ServerClient.Utils;

internal class StreamingJsonContext : IDisposable, IAsyncDisposable
{
    private readonly HttpResponseMessage _response;
    private Stream? _stream;

    private StreamWithCrLfEnding? _current;

    public StreamingJsonContext(HttpResponseMessage response)
    {
        _response = response;
    }

    public async Task<Stream> ReadCrLfJsonAsync(CancellationToken ct)
    {
        _stream ??= await _response.Content.ReadAsStreamAsync(ct);

        if (_current is null)
        {
            _current = new StreamWithCrLfEnding(_stream);
        }
        else
        {
            var newCurrent = new StreamWithCrLfEnding(_current._leftoverBytes!, _current._leftoverBytesLength, _stream);
            _current = newCurrent;
        }

        return _current;
    }

    public void Dispose()
    {
        _response.Dispose();
        _stream!.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        _response.Dispose();
        await _stream!.DisposeAsync();
    }
}