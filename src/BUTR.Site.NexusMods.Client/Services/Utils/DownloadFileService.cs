using Microsoft.JSInterop;

using System;
using System.IO;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services
{
    public sealed class DownloadFileService : IAsyncDisposable
    {
        private readonly Lazy<ValueTask<IJSUnmarshalledObjectReference>> _moduleTask;

        public DownloadFileService(IJSRuntime runtime)
        {
            _moduleTask = new(() => runtime.InvokeAsync<IJSUnmarshalledObjectReference>("import", "../js/utils.js"));
        }

        public async ValueTask<Stream> DownloadFileAsync(string file, string contentType, Stream compressed)
        {
            var module = await _moduleTask.Value;
            var streamReference = await module.InvokeAsync<IJSStreamReference>("downloadFile", file, contentType, new DotNetStreamReference(compressed));
            return await streamReference.OpenReadStreamAsync();
        }
        public async ValueTask<Stream> DownloadFileAsync(string file, string contentType, byte[] compressed)
        {
            var module = await _moduleTask.Value;
            var streamReference = await module.InvokeAsync<IJSStreamReference>("downloadFile", file, contentType, compressed);
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
}