using BUTR.Site.NexusMods.Server.Models;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services;

public sealed class CrashReporterClient
{
    private readonly HttpClient _httpClient;

    public CrashReporterClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<string> GetCrashReportAsync(string id, CancellationToken ct) => await _httpClient.GetStringAsync($"{id}.html", ct);

    public async Task<HashSet<string>> GetCrashReportNamesAsync(CancellationToken ct) => await _httpClient.GetFromJsonAsync<HashSet<string>>("getallfilenames", ct) ?? new HashSet<string>();

    public async Task<ImmutableArray<FilenameDate>> GetCrashReportDatesAsync(ImmutableArray<string> filenames, CancellationToken ct)
    {
        var response = await _httpClient.PostAsJsonAsync<ImmutableArray<string>>("getfilenamedates", filenames, ct);
        return await response.Content.ReadFromJsonAsync<ImmutableArray<FilenameDate>?>(cancellationToken: ct) ?? ImmutableArray<FilenameDate>.Empty;
    }
}