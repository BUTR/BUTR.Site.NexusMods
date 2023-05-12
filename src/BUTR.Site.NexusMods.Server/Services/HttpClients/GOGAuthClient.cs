using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services;

public sealed class GOGAuthClient
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
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public string GetOAuth2Url() => OAuth2Url;

    public async Task<GOGOAuthTokens?> CreateTokens(string code, CancellationToken ct)
    {
        var url = $"token?client_id={ClientId}&client_secret={ClientSecret}&grant_type=authorization_code&code={code}&redirect_uri={RedirectUri}";
        var response = await _httpClient.GetFromJsonAsync<TokenResponse>(url, ct);
        if (response is null) return null;
        return new GOGOAuthTokens(response.UserId, response.AccessToken, response.RefreshToken, DateTimeOffset.UtcNow.AddMinutes(response.ExpiresIn ?? 0));
    }

    public async Task<GOGOAuthTokens?> GetOrRefreshTokens(GOGOAuthTokens tokens, CancellationToken ct)
    {
        if (DateTimeOffset.Now <= tokens.ExpiresAt)
            return tokens;

        var url = $"token?client_id={ClientId}&client_secret={ClientSecret}&grant_type=refresh_token&refresh_token={tokens.RefreshToken}";
        var responseData = await _httpClient.GetFromJsonAsync<TokenResponse>(url, ct);
        if (responseData is null) return null;

        return new GOGOAuthTokens(responseData.UserId, responseData.AccessToken, responseData.RefreshToken, DateTimeOffset.UtcNow.AddMinutes(responseData.ExpiresIn ?? 0));
    }
}