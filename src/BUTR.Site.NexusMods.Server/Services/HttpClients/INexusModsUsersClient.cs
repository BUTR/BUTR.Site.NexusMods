using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Options;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace BUTR.Site.NexusMods.Server.Services;

public sealed record NexusModsOAuthTokens(string AccessToken, string? RefreshToken);

public sealed record NexusModsUserInfo(
    [property: JsonPropertyName("sub")] string UserId,
    [property: JsonPropertyName("name")] NexusModsUserName Name,
    [property: JsonPropertyName("email")] NexusModsUserEMail Email,
    [property: JsonPropertyName("avatar")] string? AvatarUrl,
    [property: JsonPropertyName("membership_roles")] string[] MembershipRoles);

public interface INexusModsUsersClient
{
    (string Url, string CodeVerifier, Guid State) GetOAuthUrl();
    Task<NexusModsOAuthTokens?> CreateTokensAsync(string code, string codeVerifier, CancellationToken ct);
    Task<NexusModsUserInfo?> GetUserInfoAsync(NexusModsOAuthTokens tokens, CancellationToken ct);
}

public sealed class NexusModsUsersClient : INexusModsUsersClient
{
    public sealed record NexusModsOAuthTokensResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("_received_at")] long ReceivedAt,
        [property: JsonPropertyName("token_type")] string? Type,
        [property: JsonPropertyName("expires_in")] ulong ExpiresIn,
        [property: JsonPropertyName("refresh_token")] string? RefreshToken,
        [property: JsonPropertyName("scope")] string? Scope,
        [property: JsonPropertyName("created_at")] long CreatedAt)
    {
        public bool IsExpired => DateTime.FromFileTimeUtc(ReceivedAt) + TimeSpan.FromSeconds(ExpiresIn) - TimeSpan.FromMinutes(5) <= DateTimeOffset.UtcNow;

    }

    private readonly HttpClient _httpClient;
    private readonly NexusModsUsersOptions _options;

    public NexusModsUsersClient(HttpClient httpClient, IOptions<NexusModsUsersOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public (string Url, string CodeVerifier, Guid State) GetOAuthUrl()
    {
        var state = Guid.NewGuid();

        // see https://www.rfc-editor.org/rfc/rfc7636#section-4.1
        var codeVerifier = Convert.ToBase64String(Encoding.ASCII.GetBytes(Guid.NewGuid().ToString("N")));

        // see https://www.rfc-editor.org/rfc/rfc7636#section-4.2
        var codeChallengeBytes = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
        var codeChallenge = Base64UrlTextEncoder.Encode(codeChallengeBytes);

        var url = new UriBuilder($"{_httpClient.BaseAddress}oauth/authorize");
        var query = HttpUtility.ParseQueryString(url.Query);
        query["response_type"] = "code";
        query["scope"] = "openid profile email";
        query["code_challenge_method"] = "S256";
        query["redirect_uri"] = _options.RedirectUri;
        query["code_challenge"] = codeChallenge;
        query["state"] = state.ToString();
        query["client_id"] = _options.ClientId;
        url.Query = query.ToString();
        return (url.ToString(), codeVerifier, state);
    }

    public async Task<NexusModsOAuthTokens?> CreateTokensAsync(string code, string codeVerifier, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "oauth/token");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
        {
            new("grant_type", "authorization_code"),
            new("client_id", _options.ClientId),
            new("redirect_uri", _options.RedirectUri),
            new("code", code),
            new("code_verifier", codeVerifier),
        });
        using var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return null;
        var tokens = await JsonSerializer.DeserializeAsync<NexusModsOAuthTokensResponse>(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        return tokens is not null ? new NexusModsOAuthTokens(tokens.AccessToken, tokens.RefreshToken) : null;
    }

    public async Task<NexusModsUserInfo?> GetUserInfoAsync(NexusModsOAuthTokens tokens, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "oauth/userinfo");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        using var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return null;
        return await JsonSerializer.DeserializeAsync<NexusModsUserInfo>(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
    }
}