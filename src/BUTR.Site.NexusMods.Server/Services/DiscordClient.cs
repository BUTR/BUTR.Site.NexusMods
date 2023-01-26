using BUTR.Site.NexusMods.Server.Options;

using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;

namespace BUTR.Site.NexusMods.Server.Services
{
    public sealed record DiscordOAuthTokens(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);

    public sealed record DiscordUserInfoUser(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("username")] string Username,
        [property: JsonPropertyName("discriminator")] int Discriminator);
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
        private readonly IDiscordStorage _storage;

        public DiscordClient(HttpClient httpClient, IOptions<DiscordOptions> options, IDiscordStorage storage)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public async Task<bool> SetGlobalMetadata(IReadOnlyList<DiscordGlobalMetadata> metadata)
        {
            using var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Put, $"https://discord.com/api/v10/applications/{_options.ClientId}/role-connections/metadata")
            {
                Headers =
                {
                    {"Authorization", $"Bot {_options.BotToken}"},
                },
                Content = new StringContent(JsonSerializer.Serialize(metadata), Encoding.UTF8, "application/json"),
            });
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

        public async Task<DiscordOAuthTokens?> GetOAuthTokens(string code)
        {
            var data = new List<KeyValuePair<string, string>>()
            {
                new("client_id", _options.ClientId),
                new("client_secret", _options.ClientSecret),
                new("redirect_uri", _options.RedirectUri),
                new("grant_type", "authorization_code"),
                new("code", code),
            };
            using var response = await _httpClient.PostAsync("https://discord.com/api/v10/oauth2/token", new FormUrlEncodedContent(data));
            var tokens = await JsonSerializer.DeserializeAsync<DiscordOAuthTokensResponse>(await response.Content.ReadAsStreamAsync());
            return tokens is not null? new DiscordOAuthTokens(tokens.AccessToken, tokens.RefreshToken, DateTimeOffset.Now + TimeSpan.FromSeconds(tokens.ExpiresIn)) : null;
        }

        public async Task<string?> GetAccessToken(int userId, DiscordOAuthTokens tokens)
        {
            if (DateTimeOffset.Now <= tokens.ExpiresAt)
                return tokens.AccessToken;
            
            var data = new List<KeyValuePair<string, string>>
            {
                new("client_id", _options.ClientId),
                new("redirect_uri", _options.RedirectUri),
                new("grant_type", "refresh_token"),
                new("refresh_token", tokens.RefreshToken),
            };
            using var response = await _httpClient.PostAsync("https://discord.com/api/v10/oauth2/token", new FormUrlEncodedContent(data));
            if (response.IsSuccessStatusCode && await JsonSerializer.DeserializeAsync<DiscordOAuthTokensResponse>(await response.Content.ReadAsStreamAsync()) is { } tokensNew)
            {
                _storage.Upsert(userId, new DiscordOAuthTokens(tokensNew.AccessToken, tokensNew.RefreshToken, DateTimeOffset.Now + TimeSpan.FromSeconds(tokensNew.ExpiresIn)));
                return tokensNew.AccessToken;
            }

            return null;
        }
        
        public async Task<DiscordUserInfo?> GetUserInfo(string accessToken)
        {
            using var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/v10/oauth2/@me")
            {
                Headers =
                {
                    {"Authorization", $"Bearer {accessToken}"}
                }
            });
            return await JsonSerializer.DeserializeAsync<DiscordUserInfo>(await response.Content.ReadAsStreamAsync());
        }
        
        public async Task<bool> PushMetadata<T>(int userId, DiscordOAuthTokens tokens, T metadata)
        {
            var accessToken = await GetAccessToken(userId, tokens);

            using var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Put, $"https://discord.com/api/v10/users/@me/applications/{_options.ClientId}/role-connection")
            {
                Headers =
                {
                    {"Authorization", $"Bearer {accessToken}"},
                },
                Content = new StringContent(JsonSerializer.Serialize(new PutMetadata<T>("BUTR", metadata)), Encoding.UTF8, "application/json"),
            });
            return response.IsSuccessStatusCode;
        }
        public async Task<bool> PushMetadata<T>(string accessToken, T metadata)
        {
            using var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Put, $"https://discord.com/api/v10/users/@me/applications/{_options.ClientId}/role-connection")
            {
                Headers =
                {
                    {"Authorization", $"Bearer {accessToken}"},
                },
                Content = new StringContent(JsonSerializer.Serialize(new PutMetadata<T>("BUTR", metadata)), Encoding.UTF8, "application/json"),
            });
            return response.IsSuccessStatusCode;
        }
    }
}