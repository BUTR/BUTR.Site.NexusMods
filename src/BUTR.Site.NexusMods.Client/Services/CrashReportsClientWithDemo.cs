using BUTR.Site.NexusMods.Client.Models;
using BUTR.Site.NexusMods.ServerClient;
using BUTR.Site.NexusMods.ServerClient.Utils;

using Microsoft.AspNetCore.Components.WebAssembly.Http;

using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services;

public sealed class CrashReportsClientWithStreaming : CrashReportsClient
{
    public CrashReportsClientWithStreaming(HttpClient client, JsonSerializerOptions options) : base(client, options) { }
    public CrashReportsClientWithStreaming(HttpClient httpClient) : base(httpClient) { }

    protected override void OnPrepareRequest(HttpClient client, HttpRequestMessage request, StringBuilder urlBuilder)
    {
        request.SetBrowserResponseStreamingEnabled(true);
        
        base.OnPrepareRequest(client, request, urlBuilder);
    }
}

public sealed class CrashReportsClientWithDemo : ICrashReportsClient
{
    private readonly ICrashReportsClient _implementation;
    private readonly ITokenContainer _tokenContainer;
    private readonly IHttpClientFactory _httpClientFactory;

    public CrashReportsClientWithDemo(IServiceProvider serviceProvider, ITokenContainer tokenContainer, IHttpClientFactory httpClientFactory)
    {
        _implementation = Program.ConfigureClient(serviceProvider, (http, opt) => new CrashReportsClientWithStreaming(http, opt));
        _tokenContainer = tokenContainer ?? throw new ArgumentNullException(nameof(tokenContainer));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public async Task<PagingStreamingData<CrashReportModel2>> PaginatedStreamingAsync(PaginatedQuery? body = null, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
        {
            var crashReports = await DemoUser.GetCrashReports(_httpClientFactory).ToListAsync(ct);
            return PagingStreamingData<CrashReportModel2>.Create(new PagingMetadata(1, (int) Math.Ceiling((double) crashReports.Count / body.PageSize), body.PageSize, crashReports.Count), crashReports.ToAsyncEnumerable(), PagingAdditionalMetadata.Empty);
        }

        return await _implementation.PaginatedStreamingAsync(new PaginatedQuery(body.Page, body.PageSize, body.Filters, body.Sotings), ct);
    }

    public async Task<CrashReportModel2PagingDataAPIResponse> PaginatedAsync(PaginatedQuery? body, CancellationToken ct)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
        {
            var crashReports = await DemoUser.GetCrashReports(_httpClientFactory).ToListAsync(ct);
            return new CrashReportModel2PagingDataAPIResponse(new CrashReportModel2PagingData(PagingAdditionalMetadata.Empty, crashReports, new PagingMetadata(1, (int) Math.Ceiling((double) crashReports.Count / body.PageSize), body.PageSize, crashReports.Count)), string.Empty);
        }

        return await _implementation.PaginatedAsync(new PaginatedQuery(body.Page, body.PageSize, body.Filters, body.Sotings), ct);
    }

    public async Task<StringIQueryableAPIResponse> AutocompleteAsync(string? modId, CancellationToken ct)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
        {
            var crashReports = await DemoUser.GetCrashReports(_httpClientFactory).ToListAsync(ct);
            return new StringIQueryableAPIResponse(crashReports.SelectMany(x => x.InvolvedModules).Where(x => x.StartsWith(modId)).ToArray(), string.Empty);
        }

        return new StringIQueryableAPIResponse((await _implementation.AutocompleteAsync(modId, ct)).Data ?? Array.Empty<string>(), string.Empty);
    }

    public async Task<StringAPIResponse> UpdateAsync(CrashReportModel2? body, CancellationToken ct)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
        {
            return new StringAPIResponse("demo", string.Empty);
        }

        return await _implementation.UpdateAsync(body, ct);
    }

    public async Task<ModuleUpdateIEnumerableAPIResponse> GetUpdatesAsync(int? tenant, CrashReportModel? body, CancellationToken ct)
    {
        return await _implementation.GetUpdatesAsync(tenant, body, ct);
    }
}