using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services;

public sealed class AssetsDelegatingHandler : DelegatingHandler
{
    private sealed class DecompressedContent : StreamContent
    {
        public DecompressedContent(HttpContent content, Stream stream) : base(stream)
        {
            foreach (var (key, value) in content.Headers)
            {
                Headers.TryAddWithoutValidation(key, value);
            }
        }
    }

    private readonly BrotliDecompressorService _brotliDecompressorService;

    public AssetsDelegatingHandler(BrotliDecompressorService brotliDecompressorService)
    {
        _brotliDecompressorService = brotliDecompressorService ?? throw new ArgumentNullException(nameof(brotliDecompressorService));
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        if (request.RequestUri?.ToString() is { } url && Path.HasExtension(url) && Path.GetExtension(url) is { } ext)
        {
            request.RequestUri = new Uri(url.Replace(ext, $"{ext}.br"));
        }

        var response = await base.SendAsync(request, ct);
        var compressed = await response.Content.ReadAsStreamAsync(ct);
        var decompressed = await _brotliDecompressorService.DecompileAsync(compressed);

        response.Content = new DecompressedContent(response.Content, decompressed);

        return response;
    }
}