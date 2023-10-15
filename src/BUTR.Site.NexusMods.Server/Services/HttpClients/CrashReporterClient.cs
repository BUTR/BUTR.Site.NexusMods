using BUTR.CrashReport.Models;
using BUTR.Site.NexusMods.Server.Models;

using Microsoft.Extensions.Options;

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
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public CrashReporterClient(HttpClient httpClient, IOptions<JsonSerializerOptions> jsonSerializerOptions)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _jsonSerializerOptions = jsonSerializerOptions.Value ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
    }

    public async Task<string> GetCrashReportAsync(CrashReportFileId id, CancellationToken ct) => await _httpClient.GetStringAsync($"{id}.html", ct);
    public async Task<CrashReportModel?> GetCrashReportModelAsync(CrashReportFileId id, CancellationToken ct) => await _httpClient.GetFromJsonAsync<CrashReportModel>($"{id}.json", ct);

    public async IAsyncEnumerable<CrashReportFileMetadata?> GetNewCrashReportMetadatasAsync(DateTime dateTime, [EnumeratorCancellation] CancellationToken ct)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "getnewcrashreports") { Content = JsonContent.Create(new { DateTime = dateTime.ToString("o") }, options: _jsonSerializerOptions) };
        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        await foreach (var entry in JsonSerializer.DeserializeAsyncEnumerable<CrashReportFileMetadata>(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct))
            yield return entry;
    }
    public async IAsyncEnumerable<CrashReportFileMetadata?> GetCrashReportMetadatasAsync(IEnumerable<CrashReportFileId> filenames, [EnumeratorCancellation] CancellationToken ct)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "getmetadata") { Content = JsonContent.Create(filenames, options: _jsonSerializerOptions) };
        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        await foreach (var entry in JsonSerializer.DeserializeAsyncEnumerable<CrashReportFileMetadata>(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct))
            yield return entry;
    }
}