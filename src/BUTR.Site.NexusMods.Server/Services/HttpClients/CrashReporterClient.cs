using BUTR.Site.NexusMods.Server.Models;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
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

    public async Task<string> GetCrashReportAsync(CrashReportFileId id, CancellationToken ct) => await _httpClient.GetStringAsync($"{id}.html", ct);

    public async Task<HashSet<CrashReportFileId>> GetCrashReportNamesAsync(CancellationToken ct) => await _httpClient.GetFromJsonAsync<HashSet<CrashReportFileId>>("getallfilenames", ct) ?? new HashSet<CrashReportFileId>();

    public async IAsyncEnumerable<FileIdDate?> GetCrashReportDatesAsync(IEnumerable<CrashReportFileId> filenames, [EnumeratorCancellation] CancellationToken ct)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "getfilenamedates") { Content = JsonContent.Create(filenames) };
        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        await foreach (var entry in JsonSerializer.DeserializeAsyncEnumerable<FileIdDate>(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct))
            yield return entry;
    }
}