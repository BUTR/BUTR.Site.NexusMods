using Microsoft.Extensions.Options;

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services;

public sealed record GOGUserInfo(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("username")] string Username);

public record GamesOwned(
    [property: JsonPropertyName("owned")] IReadOnlyList<uint?> Owned
);

public interface IGOGEmbedClient
{
    Task<GOGUserInfo?> GetUserInfoAsync(string token, CancellationToken ct);
    Task<GamesOwned?> GetGamesAsync(string token, CancellationToken ct);
}

public sealed class GOGEmbedClient : IGOGEmbedClient
{
    public record UserInfo(
        [property: JsonPropertyName("userId")] string UserId,
        [property: JsonPropertyName("username")] string Username
    );

    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public GOGEmbedClient(HttpClient httpClient, IOptions<JsonSerializerOptions> jsonSerializerOptions)
    {
        _httpClient = httpClient;
        _jsonSerializerOptions = jsonSerializerOptions.Value;
    }

    public async Task<GOGUserInfo?> GetUserInfoAsync(string token, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/userData.json");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        using var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return null;

        var response2 = JsonSerializer.Deserialize<UserInfo>(await response.Content.ReadAsStreamAsync(ct), _jsonSerializerOptions);
        if (response2 is null) return null;
        return new GOGUserInfo(response2.UserId, response2.Username);
    }

    public async Task<GamesOwned?> GetGamesAsync(string token, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/user/data/games");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        using var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return null;

        return JsonSerializer.Deserialize<GamesOwned>(await response.Content.ReadAsStreamAsync(ct), _jsonSerializerOptions);
    }
}