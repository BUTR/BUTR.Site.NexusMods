using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Utils;

using ComposableAsync;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

using RateLimiter;

using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services
{
    public sealed class NexusModsAPIClient
    {
        private readonly HttpClient _httpClient;
        private readonly IDistributedCache _cache;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly DistributedCacheEntryOptions _apiKeyExpiration = new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1) };
        private readonly DistributedCacheEntryOptions _expiration = new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) };
        private readonly SemaphoreSlim _lock = new(1, 1);
        private TimeLimiter _timeLimiter = TimeLimiter.GetFromMaxCountByInterval(30, TimeSpan.FromSeconds(1));

        public NexusModsAPIClient(HttpClient httpClient, IDistributedCache cache, IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _jsonSerializerOptions = jsonSerializerOptions.Value ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
        }
        
        private static string HashString(string value)
        {
            Span<byte> data2 = stackalloc byte[Encoding.UTF8.GetByteCount(value)];
            Encoding.UTF8.GetBytes(value, data2);
            Span<byte> data = stackalloc byte[64];
            SHA512.HashData(data2, data);
            return Convert.ToBase64String(data);
        }

        public async Task<NexusModsValidateResponse?> ValidateAPIKeyAsync(string apiKey)
        {
            var apiKeyKey = HashString(apiKey);
            try
            {
                if (await _cache.GetStringAsync(apiKeyKey) is { } json)
                    return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<NexusModsValidateResponse>(json, _jsonSerializerOptions);
            
                var request = new HttpRequestMessage(HttpMethod.Get, "v1/users/validate.json");
                request.Headers.Add("apikey", apiKey);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    return null;
                    
                json = await response.Content.ReadAsStringAsync();
                var responseType = JsonSerializer.Deserialize<NexusModsValidateResponse>(json, _jsonSerializerOptions);
                if (responseType is null || responseType.Key != apiKey)
                    return null;

                await _cache.SetStringAsync(apiKeyKey, json, _apiKeyExpiration);
                return responseType;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Task<NexusModsModInfoResponse?> GetModAsync(string gameDomain, int modId, string apiKey) =>
            GetCachedWithTimeLimitAsync<NexusModsModInfoResponse?>($"/v1/games/{gameDomain}/mods/{modId}.json", apiKey);

        public Task<NexusModsModFilesResponse?> GetModFileInfosAsync(string gameDomain, int modId, string apiKey) =>
            GetCachedWithTimeLimitAsync<NexusModsModFilesResponse?>($"/v1/games/{gameDomain}/mods/{modId}/files.json?category=main", apiKey);

        public Task<NexusModsDownloadLinkResponse[]?> GetModFileLinksAsync(string gameDomain, int modId, int fileId, string apiKey) =>
            GetCachedWithTimeLimitAsync<NexusModsDownloadLinkResponse[]>($"/v1/games/{gameDomain}/mods/{modId}/files/{fileId}/download_link.json", apiKey);

        public async Task<NexusModsUpdatedModsResponse[]?> GetAllModUpdatesAsync(string gameDomain, string apiKey)
        {
            try
            {
                await _lock.WaitAsync();
                await _timeLimiter;

                var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/games/{gameDomain}/mods/updated.json?period=1w");
                request.Headers.Add("apikey", apiKey);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await _httpClient.SendAsync(request);
                _timeLimiter = ParseResponseLimits(response);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var mod = JsonSerializer.Deserialize<NexusModsUpdatedModsResponse[]>(json, _jsonSerializerOptions);
                if (mod is null)
                {
                    return null;
                }

                return response.IsSuccessStatusCode ? mod : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<TResponse?> GetCachedWithTimeLimitAsync<TResponse>(string url, string apiKey) where TResponse : class?
        {
            var apiKeyKey = HashString(apiKey);
            try
            {
                if (await _cache.GetStringAsync(url) is { } json)
                {
                    return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<TResponse>(json, _jsonSerializerOptions);
                }

                try
                {
                    await _lock.WaitAsync();
                    await _timeLimiter;

                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Add("apikey", apiKey);
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var response = await _httpClient.SendAsync(request);
                    _timeLimiter = ParseResponseLimits(response);
                    if (!response.IsSuccessStatusCode)
                    {
                        await _cache.SetStringAsync(url, "", _expiration);
                        return null;
                    }
                    json = await response.Content.ReadAsStringAsync();
                    var mod = JsonSerializer.Deserialize<TResponse>(json, _jsonSerializerOptions);
                    if (mod is null)
                    {
                        await _cache.SetStringAsync(url, "", _expiration);
                        return null;
                    }

                    await _cache.SetStringAsync(url, json, _expiration);
                    return response.IsSuccessStatusCode ? mod : null;
                }
                finally
                {
                    _lock.Release();
                }
            }
            catch (Exception)
            {
                await _cache.RemoveAsync(apiKeyKey);
                return null;
            }
        }

        private static TimeLimiter ParseResponseLimits(HttpResponseMessage response)
        {
            // A 429 status code can also be served by nginx if the client sends more than 30 requests per second.
            // Nginx will however allow bursts over this for very short periods of time.
            var constraint = new CountByIntervalAwaitableConstraint(30, TimeSpan.FromSeconds(1));


            var hourlyLimit = int.TryParse(response.Headers.GetValues("X-RL-Hourly-Limit").FirstOrDefault(), out var hourlyLimitVal) ? hourlyLimitVal : 0;
            var hourlyLimitConstraint = new CountByIntervalAwaitableConstraint(hourlyLimit, TimeSpan.FromHours(1));

            var hourlyRemaining = int.TryParse(response.Headers.GetValues("X-RL-Hourly-Remaining").FirstOrDefault(), out var hourlyRemainingVal) ? hourlyRemainingVal : 0;
            var hourlyReset = DateTime.TryParse(response.Headers.GetValues("X-RL-Hourly-Reset").FirstOrDefault(), out var hourlyResetVal) ? hourlyResetVal : DateTime.UtcNow;
            var hourlyTimeLeft = hourlyReset - DateTime.UtcNow;
            var hourlyRemainingConstraint = hourlyRemaining == 0
                ? (IAwaitableConstraint) new BlockUntilDateConstraint(hourlyReset)
                : (IAwaitableConstraint) new CountByIntervalAwaitableConstraint(hourlyRemaining, hourlyTimeLeft);


            var dailyLimit = int.TryParse(response.Headers.GetValues("X-RL-Daily-Limit").FirstOrDefault(), out var dailyLimitVal) ? dailyLimitVal : 0;
            var dailyLimitConstraint = new CountByIntervalAwaitableConstraint(dailyLimit, TimeSpan.FromDays(1));

            var dailyRemaining = int.TryParse(response.Headers.GetValues("X-RL-Daily-Remaining").FirstOrDefault(), out var dailyRemainingVal) ? dailyRemainingVal : 0;
            var dailyReset = DateTime.TryParse(response.Headers.GetValues("X-RL-Daily-Reset").FirstOrDefault(), out var dailyResetVal) ? dailyResetVal : DateTime.UtcNow;
            var dailyTimeLeft = dailyReset - DateTime.UtcNow;
            var dailyRemainingConstraint = dailyRemaining == 0
                ? (IAwaitableConstraint) new BlockUntilDateConstraint(dailyReset)
                : (IAwaitableConstraint) new CountByIntervalAwaitableConstraint(dailyRemaining, dailyTimeLeft);


            return TimeLimiter.Compose(constraint, hourlyLimitConstraint, hourlyRemainingConstraint, dailyLimitConstraint, dailyRemainingConstraint);
        }
    }
}