using BUTR.Site.NexusMods.Server.Options;

using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace BUTR.Site.NexusMods.Server.Services;

public sealed class GitHubClient
{
    public sealed record GitHubOAuthTokensResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("scope")] string Scope,
        [property: JsonPropertyName("token_type")] string TokenType);

    private readonly HttpClient _httpClient;
    private readonly GitHubOptions _options;

    public GitHubClient(HttpClient httpClient, IOptions<GitHubOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public (string Url, Guid State) GetOAuthUrl()
    {
        var state = Guid.NewGuid();

        var url = new UriBuilder($"{_httpClient.BaseAddress}login/oauth/authorize");
        var query = HttpUtility.ParseQueryString(url.Query);
        query["client_id"] = _options.ClientId;
        query["redirect_uri"] = _options.RedirectUri;
        query["state"] = state.ToString();
        url.Query = query.ToString();
        return (url.ToString(), state);
    }

    public async Task<GitHubOAuthTokens?> CreateTokensAsync(string code, CancellationToken ct)
    {
        var data = new List<KeyValuePair<string, string>>
        {
            new("client_id", _options.ClientId),
            new("client_secret", _options.ClientSecret),
            new("redirect_uri", _options.RedirectUri),
            new("code", code),
        };
        var post = new HttpRequestMessage(HttpMethod.Post, "login/oauth/access_token");
        post.Content = new FormUrlEncodedContent(data);
        post.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        using var response = await _httpClient.SendAsync(post, ct);
        var tokens = await JsonSerializer.DeserializeAsync<GitHubOAuthTokensResponse>(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        return tokens is not null ? new GitHubOAuthTokens(tokens.AccessToken) : null;
    }

    /*
    public async Task<DiscordOAuthTokens?> GetOrRefreshTokensAsync(DiscordOAuthTokens tokens, CancellationToken ct)
    {
        if (DateTimeOffset.UtcNow <= tokens.ExpiresAt)
            return tokens;

        var data = new List<KeyValuePair<string, string>>
        {
            new("client_id", _options.ClientId),
            new("redirect_uri", _options.RedirectUri),
            new("grant_type", "refresh_token"),
            new("refresh_token", tokens.RefreshToken),
        };
        using var response = await _httpClient.PostAsync("v10/oauth2/token", new FormUrlEncodedContent(data), ct);
        if (!response.IsSuccessStatusCode) return null;
        var responseData = await JsonSerializer.DeserializeAsync<DiscordClient.DiscordOAuthTokensResponse>(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        if (responseData is null) return null;

        return new DiscordOAuthTokens(responseData.AccessToken, responseData.RefreshToken, DateTimeOffset.UtcNow + TimeSpan.FromSeconds(responseData.ExpiresIn));
    }
    */
}