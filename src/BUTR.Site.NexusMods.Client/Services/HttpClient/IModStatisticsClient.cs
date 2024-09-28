using BUTR.Site.NexusMods.Client.Models;

using Microsoft.Extensions.Options;

using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services;

public interface IModStatisticsClient
{
    Task<ModStatsRootModel?> GetModStatisticsAsync(int gameId, int modId, CancellationToken ct);
}

public sealed class ModStatisticsClient : IModStatisticsClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public ModStatisticsClient(HttpClient httpClient, IOptions<JsonSerializerOptions> jsonSerializerOptions)
    {
        _httpClient = httpClient;
        _jsonSerializerOptions = jsonSerializerOptions.Value;
    }

    public async Task<ModStatsRootModel?> GetModStatisticsAsync(int gameId, int modId, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"mod_monthly_stats/{gameId}/{modId}.json");
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        if (!response.IsSuccessStatusCode) return null;
        return await JsonSerializer.DeserializeAsync<ModStatsRootModel>(await response.Content.ReadAsStreamAsync(ct), _jsonSerializerOptions, ct);
    }
}