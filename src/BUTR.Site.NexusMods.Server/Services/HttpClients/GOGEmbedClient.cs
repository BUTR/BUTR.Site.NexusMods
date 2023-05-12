using BUTR.Site.NexusMods.Server.Controllers;

using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services;

public sealed class GOGEmbedClient
{
    public record UserInfo(
        [property: JsonPropertyName("userId")] string UserId,
        [property: JsonPropertyName("username")] string Username
    );

    public record GamesOwned(
        [property: JsonPropertyName("owned")] IReadOnlyList<int?> Owned
    );


    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public GOGEmbedClient(HttpClient httpClient, IOptions<JsonSerializerOptions> jsonSerializerOptions)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _jsonSerializerOptions = jsonSerializerOptions.Value ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<GOGUserInfo?> GetUserInfo(string token, CancellationToken ct)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/userData.json");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync(ct);
        var response2 = JsonSerializer.Deserialize<UserInfo>(json, _jsonSerializerOptions);
        if (response2 is null) return null;
        return new GOGUserInfo(response2.UserId, response2.Username);
    }

    public async Task<GamesOwned?> GetGames(string token, CancellationToken ct)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/user/data/games");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<GamesOwned>(json, _jsonSerializerOptions);
    }
}