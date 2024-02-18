using Microsoft.Extensions.Options;

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

public interface IGitHubAPIClient
{
    Task<GitHubUserInfo?> GetUserInfoAsync(GitHubOAuthTokens tokens, CancellationToken ct);
}

public sealed class GitHubAPIClient : IGitHubAPIClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public GitHubAPIClient(HttpClient httpClient, IOptions<JsonSerializerOptions> jsonSerializerOptions)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _jsonSerializerOptions = jsonSerializerOptions.Value ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<GitHubUserInfo?> GetUserInfoAsync(GitHubOAuthTokens tokens, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "user")
        {
            Headers =
            {
                Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken)
            }
        };
        using var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return null;
        return await JsonSerializer.DeserializeAsync<GitHubUserInfo>(await response.Content.ReadAsStreamAsync(ct), _jsonSerializerOptions, cancellationToken: ct);
    }
}