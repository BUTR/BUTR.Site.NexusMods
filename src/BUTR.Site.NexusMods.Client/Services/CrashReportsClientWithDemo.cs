using BUTR.Site.NexusMods.Client.Models;
using BUTR.Site.NexusMods.ServerClient;

using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services;

public sealed class CrashReportsClientWithDemo : ICrashReportsClient
{
    private readonly ICrashReportsClient _implementation;
    private readonly ITokenContainer _tokenContainer;
    private readonly IHttpClientFactory _httpClientFactory;

    public CrashReportsClientWithDemo(IServiceProvider serviceProvider, ITokenContainer tokenContainer, IHttpClientFactory httpClientFactory)
    {
        _implementation = Program.ConfigureClient(serviceProvider, (http, opt) => new CrashReportsClient(http, opt));
        _tokenContainer = tokenContainer ?? throw new ArgumentNullException(nameof(tokenContainer));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public async Task<CrashReportModelPagingDataAPIResponse> PaginatedAsync(PaginatedQuery? body, CancellationToken ct)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
        {
            var crashReports = await DemoUser.GetCrashReports(_httpClientFactory).ToListAsync(ct);
            return new CrashReportModelPagingDataAPIResponse(new CrashReportModelPagingData(0, crashReports, new PagingMetadata(1, (int) Math.Ceiling((double) crashReports.Count / body.PageSize), body.PageSize, crashReports.Count)), string.Empty);
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

    public async Task<StringAPIResponse> UpdateAsync(CrashReportModel? body, CancellationToken ct)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
        {
            return new StringAPIResponse("demo", string.Empty);
        }

        return await _implementation.UpdateAsync(body, ct);
    }
}