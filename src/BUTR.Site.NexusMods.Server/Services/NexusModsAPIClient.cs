using BUTR.Site.NexusMods.Server.Models;

using ComposableAsync;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

using RateLimiter;

using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services
{
    public class NexusModsAPIClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDistributedCache _cache;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly DistributedCacheEntryOptions _expiration = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };
        private readonly SemaphoreSlim _lock = new(1, 1);
        private TimeLimiter _timeLimiter = TimeLimiter.GetFromMaxCountByInterval(30, TimeSpan.FromSeconds(1));

        public NexusModsAPIClient(IHttpClientFactory httpClientFactory, IDistributedCache cache, IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _jsonSerializerOptions = jsonSerializerOptions.Value ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
        }

        public async Task<NexusModsValidateResponse?> ValidateAPIKey(string apiKey)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "v1/users/validate.json");
                request.Headers.Add("apikey", apiKey);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var httpClient = _httpClientFactory.CreateClient("NexusModsAPI");
                var response = await httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    return null;
                var responseType = await response.Content.ReadFromJsonAsync<NexusModsValidateResponse>();
                if (responseType is null || responseType.Key != apiKey)
                    return null;

                return responseType;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<NexusModsModInfoResponse?> GetMod(string gameDomain, int modId, string apiKey)
        {
            try
            {
                var key = $"{gameDomain}:{modId}";
                if (await _cache.GetStringAsync(key) is { } modJson)
                {
                    return string.IsNullOrEmpty(modJson) ? null : JsonSerializer.Deserialize<NexusModsModInfoResponse>(modJson, _jsonSerializerOptions);
                }

                try
                {
                    await _lock.WaitAsync();
                    await _timeLimiter;

                    var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/games/{gameDomain}/mods/{modId}.json");
                    request.Headers.Add("apikey", apiKey);
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var httpClient = _httpClientFactory.CreateClient("NexusModsAPI");
                    var response = await httpClient.SendAsync(request);
                    _timeLimiter = ParseResponseLimits(response);
                    if (!response.IsSuccessStatusCode)
                    {
                        await _cache.SetStringAsync(key, "", _expiration);
                        return null;
                    }
                    modJson = await response.Content.ReadAsStringAsync();
                    var mod = JsonSerializer.Deserialize<NexusModsModInfoResponse>(modJson, _jsonSerializerOptions);
                    if (mod is null)
                    {
                        await _cache.SetStringAsync(key, "", _expiration);
                        return null;
                    }

                    await _cache.SetStringAsync(key, modJson, _expiration);
                    return response.IsSuccessStatusCode ? mod : null;
                }
                finally
                {
                    _lock.Release();
                }
            }
            catch (Exception)
            {
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
            var hourlyRemainingConstraint = new CountByIntervalAwaitableConstraint(hourlyRemaining, hourlyTimeLeft);


            var dailyLimit = int.TryParse(response.Headers.GetValues("X-RL-Daily-Limit").FirstOrDefault(), out var dailyLimitVal) ? dailyLimitVal : 0;
            var dailyLimitConstraint = new CountByIntervalAwaitableConstraint(dailyLimit, TimeSpan.FromDays(1));

            var dailyRemaining = int.TryParse(response.Headers.GetValues("X-RL-Daily-Remaining").FirstOrDefault(), out var dailyRemainingVal) ? dailyRemainingVal : 0;
            var dailyReset = DateTime.TryParse(response.Headers.GetValues("X-RL-Daily-Reset").FirstOrDefault(), out var dailyResetVal) ? dailyResetVal : DateTime.UtcNow;
            var dailyTimeLeft = dailyReset - DateTime.UtcNow;
            var dailyRemainingConstraint = new CountByIntervalAwaitableConstraint(dailyRemaining, dailyTimeLeft);


            return TimeLimiter.Compose(constraint, hourlyLimitConstraint, hourlyRemainingConstraint, dailyLimitConstraint, dailyRemainingConstraint);
        }
    }
}