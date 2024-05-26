using BUTR.Site.NexusMods.Server.Models;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services;

public interface INexusModsAPIv2Client
{
    Task<NexusModsUserId> GetUserIdAsync(NexusModsApiKey apiKey, NexusModsUserName username, CancellationToken ct);
}

public sealed class NexusModsAPIv2Client : INexusModsAPIv2Client
{
    private record GraphQLQuery
    {
        public string Query { get; init; }
    }

    private record GraphQLResponse<TData>(TData Data);
    private record GraphQLGetUserByNameResponse
    {
        public string MemberId { get; init; }
    }


    private readonly HttpClient _httpClient;
    private readonly IDistributedCache _cache;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly DistributedCacheEntryOptions _expiration = new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) };

    public NexusModsAPIv2Client(HttpClient httpClient, IDistributedCache cache, IOptions<JsonSerializerOptions> jsonSerializerOptions)
    {
        _httpClient = httpClient;
        _cache = cache;
        _jsonSerializerOptions = jsonSerializerOptions.Value;
    }

    private static string HashString(string value)
    {
        Span<byte> data2 = stackalloc byte[Encoding.UTF8.GetByteCount(value)];
        Encoding.UTF8.GetBytes(value, data2);
        Span<byte> data = stackalloc byte[64];
        SHA512.HashData(data2, data);
        return Convert.ToBase64String(data);
    }

    public async Task<NexusModsUserId> GetUserIdAsync(NexusModsApiKey apiKey, NexusModsUserName username, CancellationToken ct)
    {
        var key = HashString($"Username:{username}");
        try
        {
            if (await _cache.GetStringAsync(key, token: ct) is { } raw)
                return string.IsNullOrEmpty(raw) ? NexusModsUserId.None : NexusModsUserId.Parse(raw);

            using var request = new HttpRequestMessage(HttpMethod.Post, "v2/graphql");
            request.Headers.Add("apikey", apiKey.Value);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(JsonSerializer.Serialize(new GraphQLQuery
            {
                Query = "{ userByName(name: \"Aragas\") { memberId } }"
            }), Encoding.UTF8, "application/json");
            using var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return NexusModsUserId.None;

            var json = await response.Content.ReadAsStringAsync(ct);
            var responseJson = JsonSerializer.Deserialize<GraphQLResponse<GraphQLGetUserByNameResponse>>(json, _jsonSerializerOptions);
            if (responseJson?.Data is null || !NexusModsUserId.TryParse(responseJson.Data.MemberId, out var userId)) return NexusModsUserId.None;

            raw = responseJson.Data.MemberId;
            await _cache.SetStringAsync(key, raw, _expiration, token: ct);
            return userId;
        }
        catch (Exception)
        {
            await _cache.RemoveAsync(key, ct);
            return NexusModsUserId.None;
        }
    }
}