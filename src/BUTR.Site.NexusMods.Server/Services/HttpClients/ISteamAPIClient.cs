using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Options;

using Microsoft.Extensions.Options;

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services;

public sealed record SteamUserInfo(
    [property: JsonPropertyName("id")] SteamUserId Id,
    [property: JsonPropertyName("username")] string Username);

public sealed record SteamWorkshopItemInfo(
    [property: JsonPropertyName("id")] SteamUserId UserId,
    [property: JsonPropertyName("mod_id")] SteamWorkshopModId ModId,
    [property: JsonPropertyName("name")] string Name);

public interface ISteamAPIClient
{
    Task<SteamUserInfo?> GetUserInfoAsync(SteamUserId steamUserId, CancellationToken ct);
    Task<bool> IsOwningGameAsync(SteamUserId steamUserId, uint appId, CancellationToken ct);
    Task<SteamWorkshopItemInfo?> GetOwnedWorkshopItemAsync(SteamUserId steamUserId, uint appId, SteamWorkshopModId workshopModId, CancellationToken ct);
    Task<List<SteamWorkshopItemInfo>> GetAllOwnedWorkshopItemAsync(SteamUserId steamUserId, uint appId, CancellationToken ct);
}

public sealed class SteamAPIClient : ISteamAPIClient
{
    public record GetUserInfoRoot(
        [property: JsonPropertyName("response")] GetUserInfoResponse Response
    );
    public record GetUserInfoResponse(
        [property: JsonPropertyName("players")] IReadOnlyList<GetUserInfoPlayer> Players
    );
    public record GetUserInfoPlayer(
        [property: JsonPropertyName("steamid")] SteamUserId SteamUserId,
        [property: JsonPropertyName("personaname")] string Personaname
    );

    public record IsOwningGameRoot(
        [property: JsonPropertyName("response")] IsOwningGameResponse Response
    );
    public record IsOwningGameResponse(
        [property: JsonPropertyName("game_count")] int? GameCount,
        [property: JsonPropertyName("games")] IReadOnlyList<IsOwningGame> Games
    );
    public record IsOwningGame(
        [property: JsonPropertyName("appid")] int? Appid
    );

    public record IsOwningWorkshopItemRoot(
        [property: JsonPropertyName("response")] IsOwningWorkshopItemResponse Response
    );
    public record IsOwningWorkshopItemResponse(
        [property: JsonPropertyName("publishedfiledetails")] IReadOnlyList<IsOwningWorkshopItem> WorkshopItems
    );
    public record IsOwningWorkshopItem(
        [property: JsonPropertyName("publishedfileid")] SteamWorkshopModId SteamWorkshopModId,
        [property: JsonPropertyName("creator")] SteamUserId SteamUserId,
        [property: JsonPropertyName("consumer_app_id")] int AppId,
        [property: JsonPropertyName("title")] string Name
    );

    private readonly HttpClient _httpClient;
    private readonly SteamAPIOptions _options;

    public SteamAPIClient(HttpClient httpClient, IOptions<SteamAPIOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<SteamUserInfo?> GetUserInfoAsync(SteamUserId steamUserId, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"ISteamUser/GetPlayerSummaries/v0002/?key={_options.APIKey}&steamids={steamUserId}");
        using var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return null;

        var data = JsonSerializer.Deserialize<GetUserInfoRoot>(await response.Content.ReadAsStreamAsync(ct));
        if (data is null) return null;

        return new SteamUserInfo(data.Response.Players[0].SteamUserId, data.Response.Players[0].Personaname);
    }

    public async Task<bool> IsOwningGameAsync(SteamUserId steamUserId, uint appId, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"IPlayerService/GetOwnedGames/v1/?key={_options.APIKey}&steamid={steamUserId}&include_appinfo=false&include_played_free_games=false&appids_filter[0]={appId}");
        using var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return false;

        var data = JsonSerializer.Deserialize<IsOwningGameRoot>(await response.Content.ReadAsStreamAsync(ct));
        if (data is null) return false;

        return data.Response.GameCount == 1;
    }

    public async Task<SteamWorkshopItemInfo?> GetOwnedWorkshopItemAsync(SteamUserId steamUserId, uint appId, SteamWorkshopModId workshopModId, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"ISteamRemoteStorage/GetPublishedFileDetails/v1/?key={_options.APIKey}");
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "itemcount", "1" },
            { "publishedfileids[0]", workshopModId.Value.ToString() },
        });
        using var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return null;

        var data = JsonSerializer.Deserialize<IsOwningWorkshopItemRoot>(await response.Content.ReadAsStreamAsync(ct));
        if (data is null) return null;

        var ownsItem = data.Response is { WorkshopItems.Count: 1 } &&
               data.Response.WorkshopItems[0].AppId == appId &&
               data.Response.WorkshopItems[0].SteamUserId == steamUserId &&
               data.Response.WorkshopItems[0].SteamWorkshopModId == workshopModId;
        if (!ownsItem) return null;

        return new SteamWorkshopItemInfo(data.Response.WorkshopItems[0].SteamUserId, data.Response.WorkshopItems[0].SteamWorkshopModId, data.Response.WorkshopItems[0].Name);
    }

    public async Task<List<SteamWorkshopItemInfo>> GetAllOwnedWorkshopItemAsync(SteamUserId steamUserId, uint appId, CancellationToken ct)
    {
        var list = new List<SteamWorkshopItemInfo>();
        for (var page = 1;; page++)
        {
            var data = await GetAllOwnedWorkshopItemAsync(steamUserId, appId, page, ct);
            if (data.Count == 0) break;
            list.AddRange(data);
        }

        return list;
    }
    
    private async Task<List<SteamWorkshopItemInfo>> GetAllOwnedWorkshopItemAsync(SteamUserId steamUserId, uint appId, int page, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"IPublishedFileService/GetUserFiles/v1/?key={_options.APIKey}&steamid={steamUserId}&appid={appId}&return_short_description=true&numperpage=100&page={page}");
        using var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return [];

        var data = JsonSerializer.Deserialize<IsOwningWorkshopItemRoot>(await response.Content.ReadAsStreamAsync(ct));
        if (data is not { Response.WorkshopItems.Count: > 0 }) return [];

        return data.Response.WorkshopItems.Select(x => new SteamWorkshopItemInfo(x.SteamUserId, x.SteamWorkshopModId, x.Name)).ToList();
    }
}