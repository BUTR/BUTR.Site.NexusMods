using BUTR.Site.NexusMods.Server.Options;

using Microsoft.Extensions.Options;

using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services;

public sealed record SteamUserInfo(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("username")] string Username);

public interface ISteamAPIClient
{
    Task<SteamUserInfo?> GetUserInfoAsync(string steamId, CancellationToken ct);
    Task<bool> IsOwningGameAsync(string steamId, uint appId, CancellationToken ct);
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
        [property: JsonPropertyName("steamid")] string Steamid,
        [property: JsonPropertyName("communityvisibilitystate")] int? Communityvisibilitystate,
        [property: JsonPropertyName("profilestate")] int? Profilestate,
        [property: JsonPropertyName("personaname")] string Personaname,
        [property: JsonPropertyName("commentpermission")] int? Commentpermission,
        [property: JsonPropertyName("profileurl")] string Profileurl,
        [property: JsonPropertyName("avatar")] string Avatar,
        [property: JsonPropertyName("avatarmedium")] string Avatarmedium,
        [property: JsonPropertyName("avatarfull")] string Avatarfull,
        [property: JsonPropertyName("avatarhash")] string Avatarhash,
        [property: JsonPropertyName("lastlogoff")] int? Lastlogoff,
        [property: JsonPropertyName("personastate")] int? Personastate,
        [property: JsonPropertyName("realname")] string Realname,
        [property: JsonPropertyName("primaryclanid")] string Primaryclanid,
        [property: JsonPropertyName("timecreated")] int? Timecreated,
        [property: JsonPropertyName("personastateflags")] int? Personastateflags,
        [property: JsonPropertyName("loccountrycode")] string Loccountrycode,
        [property: JsonPropertyName("locstatecode")] string Locstatecode,
        [property: JsonPropertyName("loccityid")] int? Loccityid
    );

    public record IsOwningGameRoot(
        [property: JsonPropertyName("response")] IsOwningGameResponse Response
    );
    public record IsOwningGameResponse(
        [property: JsonPropertyName("game_count")] int? GameCount,
        [property: JsonPropertyName("games")] IReadOnlyList<IsOwningGameGame> Games
    );
    public record IsOwningGameGame(
        [property: JsonPropertyName("appid")] int? Appid,
        [property: JsonPropertyName("playtime_2weeks")] int? Playtime2weeks,
        [property: JsonPropertyName("playtime_forever")] int? PlaytimeForever,
        [property: JsonPropertyName("playtime_windows_forever")] int? PlaytimeWindowsForever,
        [property: JsonPropertyName("playtime_mac_forever")] int? PlaytimeMacForever,
        [property: JsonPropertyName("playtime_linux_forever")] int? PlaytimeLinuxForever,
        [property: JsonPropertyName("rtime_last_played")] int? RtimeLastPlayed
    );

    private readonly HttpClient _httpClient;
    private readonly SteamAPIOptions _options;

    public SteamAPIClient(HttpClient httpClient, IOptions<SteamAPIOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<SteamUserInfo?> GetUserInfoAsync(string steamId, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"ISteamUser/GetPlayerSummaries/v0002/?key={_options.APIKey}&steamids={steamId}");
        using var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return null;

        var data = JsonSerializer.Deserialize<GetUserInfoRoot>(await response.Content.ReadAsStreamAsync(ct));
        if (data is null) return null;

        return new SteamUserInfo(data.Response.Players[0].Steamid, data.Response.Players[0].Personaname);
    }

    public async Task<bool> IsOwningGameAsync(string steamId, uint appId, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"IPlayerService/GetOwnedGames/v1/?key={_options.APIKey}&steamid={steamId}&include_appinfo=false&include_played_free_games=false&appids_filter[0]={appId}");
        using var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return false;

        var data = JsonSerializer.Deserialize<IsOwningGameRoot>(await response.Content.ReadAsStreamAsync(ct));
        if (data is null) return false;

        return data.Response.GameCount == 1;
    }
}