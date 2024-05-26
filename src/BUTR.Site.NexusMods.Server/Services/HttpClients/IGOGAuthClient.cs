using Microsoft.Extensions.Options;

using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services;

public interface IGOGAuthClient
{
    string GetOAuth2Url();
    Task<GOGOAuthTokens?> CreateTokensAsync(string code, CancellationToken ct);
    Task<GOGOAuthTokens?> GetOrRefreshTokensAsync(GOGOAuthTokens tokens, CancellationToken ct);
}

public sealed class GOGAuthClient : IGOGAuthClient
{
    public record TokenResponse(
        [property: JsonPropertyName("expires_in")] int? ExpiresIn,
        [property: JsonPropertyName("scope")] string Scope,
        [property: JsonPropertyName("token_type")] string TokenType,
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("user_id")] string UserId,
        [property: JsonPropertyName("refresh_token")] string RefreshToken,
        [property: JsonPropertyName("session_id")] string SessionId
    );

    private const string ClientId = "46899977096215655";
    private const string ClientSecret = "9d85c43b1482497dbbce61f6e4aa173a433796eeae2ca8c5f6129f2dc4de46d9";
    private const string RedirectUri = "https%3A%2F%2Fembed.gog.com%2Fon_login_success%3Forigin%3Dclient";
    private const string OAuth2Url = $"https://login.gog.com/auth?client_id={ClientId}&layout=client2&redirect_uri={RedirectUri}&response_type=code";

    private readonly HttpClient _httpClient;

    public GOGAuthClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public string GetOAuth2Url() => OAuth2Url;

    public async Task<GOGOAuthTokens?> CreateTokensAsync(string code, CancellationToken ct)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"token?client_id={ClientId}&client_secret={ClientSecret}&grant_type=authorization_code&code={code}&redirect_uri={RedirectUri}");
        var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return null;

        var data = JsonSerializer.Deserialize<TokenResponse>(await response.Content.ReadAsStreamAsync(ct));
        if (data is null) return null;

        return new GOGOAuthTokens(data.UserId, data.AccessToken, data.RefreshToken, DateTimeOffset.UtcNow.AddMinutes(data.ExpiresIn ?? 0));
    }

    public async Task<GOGOAuthTokens?> GetOrRefreshTokensAsync(GOGOAuthTokens tokens, CancellationToken ct)
    {
        if (DateTimeOffset.UtcNow <= tokens.ExpiresAt)
            return tokens;

        var request = new HttpRequestMessage(HttpMethod.Get, $"token?client_id={ClientId}&client_secret={ClientSecret}&grant_type=refresh_token&refresh_token={tokens.RefreshToken}");
        var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return null;

        var data = JsonSerializer.Deserialize<TokenResponse>(await response.Content.ReadAsStreamAsync(ct));
        if (data is null) return null;

        return new GOGOAuthTokens(data.UserId, data.AccessToken, data.RefreshToken, DateTimeOffset.UtcNow.AddMinutes(data.ExpiresIn ?? 0));
    }
}