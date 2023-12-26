using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services;

public sealed record GitHubUserInfo(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("login")] string Login);

public sealed class GitHubAPIClient
{
    private readonly HttpClient _httpClient;

    public GitHubAPIClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }
    
    public async Task<GitHubUserInfo?> GetUserInfoAsync(GitHubOAuthTokens tokens, CancellationToken ct)
    {
        using var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "user")
        {
            Headers =
            {
                Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken)
            }
        }, ct);
        return await JsonSerializer.DeserializeAsync<GitHubUserInfo>(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
    }
}