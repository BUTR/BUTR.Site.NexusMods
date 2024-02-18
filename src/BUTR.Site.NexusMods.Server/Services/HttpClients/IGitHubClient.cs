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

public interface IGitHubClient
{
    (string Url, Guid State) GetOAuthUrl();
    Task<GitHubOAuthTokens?> CreateTokensAsync(string code, CancellationToken ct);
}

public sealed class GitHubClient : IGitHubClient
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
        using var request = new HttpRequestMessage(HttpMethod.Post, "login/oauth/access_token")
        {
            Headers =
            {
                Accept = { new MediaTypeWithQualityHeaderValue("application/json") }
            },
            Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new("client_id", _options.ClientId),
                new("client_secret", _options.ClientSecret),
                new("redirect_uri", _options.RedirectUri),
                new("code", code),
            })
        };
        using var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return null;
        var tokens = await JsonSerializer.DeserializeAsync<GitHubOAuthTokensResponse>(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        return tokens is not null ? new GitHubOAuthTokens(tokens.AccessToken) : null;
    }
}