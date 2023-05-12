using BUTR.Site.NexusMods.Server.Options;

using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace BUTR.Site.NexusMods.Server.Services;

public sealed record DiscordUserInfoUser(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("discriminator")] string Discriminator);
public sealed record DiscordUserInfo(
    [property: JsonPropertyName("user")] DiscordUserInfoUser User);

public sealed record DiscordGlobalMetadata(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("type")] int Type);

public sealed class DiscordClient
{
    private sealed record PutMetadata<T>(
        [property: JsonPropertyName("platform_name")] string PlatformName,
        [property: JsonPropertyName("metadata")] T Metadata);

    public sealed record DiscordOAuthTokensResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("refresh_token")] string RefreshToken,
        [property: JsonPropertyName("expires_in")] ulong ExpiresIn);


    private readonly HttpClient _httpClient;
    private readonly DiscordOptions _options;

    public DiscordClient(HttpClient httpClient, IOptions<DiscordOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<bool> SetGlobalMetadata(IReadOnlyList<DiscordGlobalMetadata> metadata, CancellationToken ct)
    {
        using var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Put, $"https://discord.com/api/v10/applications/{_options.ClientId}/role-connections/metadata")
        {
            Headers =
            {
                {"Authorization", $"Bot {_options.BotToken}"},
            },
            Content = new StringContent(JsonSerializer.Serialize(metadata), Encoding.UTF8, "application/json"),
        }, ct);
        return response.IsSuccessStatusCode;
    }

    public (string Url, Guid State) GetOAuthUrl()
    {
        var state = Guid.NewGuid();

        var url = new UriBuilder("https://discord.com/api/oauth2/authorize");
        var query = HttpUtility.ParseQueryString(url.Query);
        query["client_id"] = _options.ClientId;
        query["redirect_uri"] = _options.RedirectUri;
        query["response_type"] = "code";
        query["state"] = state.ToString();
        query["scope"] = "role_connections.write identify";
        query["prompt"] = "consent";
        url.Query = query.ToString();
        return (url.ToString(), state);
    }

    public async Task<DiscordOAuthTokens?> CreateTokens(string code, CancellationToken ct)
    {
        var data = new List<KeyValuePair<string, string>>()
        {
            new("client_id", _options.ClientId),
            new("client_secret", _options.ClientSecret),
            new("redirect_uri", _options.RedirectUri),
            new("grant_type", "authorization_code"),
            new("code", code),
        };
        using var response = await _httpClient.PostAsync("https://discord.com/api/v10/oauth2/token", new FormUrlEncodedContent(data), ct);
        var tokens = await JsonSerializer.DeserializeAsync<DiscordOAuthTokensResponse>(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        return tokens is not null ? new DiscordOAuthTokens(tokens.AccessToken, tokens.RefreshToken, DateTimeOffset.Now + TimeSpan.FromSeconds(tokens.ExpiresIn)) : null;
    }

    public async Task<DiscordOAuthTokens?> GetOrRefreshTokens(DiscordOAuthTokens tokens, CancellationToken ct)
    {
        if (DateTimeOffset.Now <= tokens.ExpiresAt)
            return tokens;

        var data = new List<KeyValuePair<string, string>>
        {
            new("client_id", _options.ClientId),
            new("redirect_uri", _options.RedirectUri),
            new("grant_type", "refresh_token"),
            new("refresh_token", tokens.RefreshToken),
        };
        using var response = await _httpClient.PostAsync("https://discord.com/api/v10/oauth2/token", new FormUrlEncodedContent(data), ct);
        if (!response.IsSuccessStatusCode) return null;
        var responseData = await JsonSerializer.DeserializeAsync<DiscordOAuthTokensResponse>(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        if (responseData is null) return null;
        
        return new DiscordOAuthTokens(responseData.AccessToken, responseData.RefreshToken, DateTimeOffset.Now + TimeSpan.FromSeconds(responseData.ExpiresIn));
    }
    
    public async Task<DiscordUserInfo?> GetUserInfo(DiscordOAuthTokens tokens, CancellationToken ct)
    {
        using var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/v10/oauth2/@me")
        {
            Headers =
            {
                {"Authorization", $"Bearer {tokens.AccessToken}"}
            }
        }, ct);
        return await JsonSerializer.DeserializeAsync<DiscordUserInfo>(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
    }

    public async Task<bool> PushMetadata<T>(DiscordOAuthTokens tokens, T metadata, CancellationToken ct)
    {
        using var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Put, $"https://discord.com/api/v10/users/@me/applications/{_options.ClientId}/role-connection")
        {
            Headers =
            {
                {"Authorization", $"Bearer {tokens.AccessToken}"},
            },
            Content = new StringContent(JsonSerializer.Serialize(new PutMetadata<T>("BUTR", metadata)), Encoding.UTF8, "application/json"),
        }, ct);
        return response.IsSuccessStatusCode;
    }
}