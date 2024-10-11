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

public interface ICrashReporterClient
{
    Task<string?> GetCrashReportAsync(TenantId tenant, CrashReportFileId id, CancellationToken ct);
    Task<string?> GetCrashReportJsonAsync(TenantId tenant, CrashReportFileId id, CancellationToken ct);
    IAsyncEnumerable<CrashReportFileMetadata> GetNewCrashReportMetadatasAsync(TenantId tenant, DateTime dateTime, CancellationToken ct);
    IAsyncEnumerable<CrashReportFileMetadata> GetCrashReportMetadatasAsync(TenantId tenant, IEnumerable<CrashReportFileId> filenames, CancellationToken ct);
}

public sealed class CrashReporterClient : ICrashReporterClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public CrashReporterClient(HttpClient httpClient, IOptions<JsonSerializerOptions> jsonSerializerOptions)
    {
        _httpClient = httpClient;
        _jsonSerializerOptions = jsonSerializerOptions.Value;
    }

    public async Task<string?> GetCrashReportAsync(TenantId tenant, CrashReportFileId id, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{tenant}/{id}.html");
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        if (!response.IsSuccessStatusCode)
            return null;
        return await response.Content.ReadAsStringAsync(ct);
    }

    public async Task<string?> GetCrashReportJsonAsync(TenantId tenant, CrashReportFileId id, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{tenant}/{id}.json");
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        if (!response.IsSuccessStatusCode)
            return null;
        return await _httpClient.GetStringAsync($"{tenant}/{id}.json", ct);
    }

    public async IAsyncEnumerable<CrashReportFileMetadata> GetNewCrashReportMetadatasAsync(TenantId tenant, DateTime dateTime, [EnumeratorCancellation] CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{tenant}/getnewcrashreports");
        request.Content = JsonContent.Create(new { DateTime = dateTime.ToString("o") }, options: _jsonSerializerOptions);
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        if (!response.IsSuccessStatusCode)
            yield break;
        await foreach (var entry in JsonSerializer.DeserializeAsyncEnumerable<CrashReportFileMetadata>(await response.Content.ReadAsStreamAsync(ct), _jsonSerializerOptions, ct))
        {
            if (entry is not null)
                yield return entry;
        }
    }
    public async IAsyncEnumerable<CrashReportFileMetadata> GetCrashReportMetadatasAsync(TenantId tenant, IEnumerable<CrashReportFileId> filenames, [EnumeratorCancellation] CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{tenant}/getmetadata");
        request.Content = JsonContent.Create(filenames, options: _jsonSerializerOptions);
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        if (!response.IsSuccessStatusCode)
            yield break;
        await foreach (var entry in JsonSerializer.DeserializeAsyncEnumerable<CrashReportFileMetadata>(await response.Content.ReadAsStreamAsync(ct), _jsonSerializerOptions, ct))
        {
            if (entry is not null)
                yield return entry;
        }
    }
}