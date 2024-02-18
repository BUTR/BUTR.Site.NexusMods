using BUTR.Site.NexusMods.Server.Models;

using HtmlAgilityPack;

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services;

public interface INexusModsClient
{
    Task<HtmlDocument?> GetArticleAsync(NexusModsGameDomain gameDomain, NexusModsArticleId articleId, CancellationToken ct);
}

public sealed class NexusModsClient : INexusModsClient
{
    private readonly HttpClient _httpClient;

    public NexusModsClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<HtmlDocument?> GetArticleAsync(NexusModsGameDomain gameDomain, NexusModsArticleId articleId, CancellationToken ct)
    {
        using var response = await _httpClient.GetAsync($"{gameDomain}/articles/{articleId}", ct);

        var doc = new HtmlDocument();
        doc.Load(await response.Content.ReadAsStreamAsync(ct));

        return doc;
    }
}