using Microsoft.JSInterop;

using System;
using System.IO;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services;

public sealed class BrotliDecompressorService : IAsyncDisposable
{
    private readonly Lazy<ValueTask<IJSUnmarshalledObjectReference>> _moduleTask;

    public BrotliDecompressorService(IJSRuntime runtime)
    {
        _moduleTask = new(() => runtime.InvokeAsync<IJSUnmarshalledObjectReference>("import", "../js/brotli.js"));
    }

    public async ValueTask<Stream> DecompileAsync(Stream compressed)
    {
        var module = await _moduleTask.Value;
        var streamReference = await module.InvokeAsync<IJSStreamReference>("decode", new DotNetStreamReference(compressed));
        return await streamReference.OpenReadStreamAsync();
    }
    public async ValueTask<Stream> DecompileAsync(byte[] compressed)
    {
        var module = await _moduleTask.Value;
        var streamReference = await module.InvokeAsync<IJSStreamReference>("decode", compressed);
        return await streamReference.OpenReadStreamAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            var module = await _moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}