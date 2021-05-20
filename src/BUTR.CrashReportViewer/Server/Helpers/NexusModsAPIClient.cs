using BUTR.CrashReportViewer.Server.Models.NexusModsAPI;

using ComposableAsync;

using Microsoft.Extensions.Caching.Distributed;

using RateLimiter;

using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.CrashReportViewer.Server.Helpers
{
    public class NexusModsAPIClient
    {
        private static JsonSerializerOptions JsonSerializerOptions { get; } = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            Converters = {new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)},
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDistributedCache _distributedCache;
        private readonly DistributedCacheEntryOptions _expiration = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };
        private readonly SemaphoreSlim _lock = new(1, 1);
        private TimeLimiter _timeLimiter = TimeLimiter.GetFromMaxCountByInterval(30, TimeSpan.FromSeconds(1));

        public NexusModsAPIClient(IHttpClientFactory httpClientFactory, IDistributedCache distributedCache)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
        }

        public async Task<NexusModsValidateResponse?> ValidateAPIKey(string apiKey)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "v1/users/validate.json");
            request.Headers.Add("apikey", apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var httpClient = _httpClientFactory.CreateClient("NexusModsAPI");
            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            else
            {
                var responseType = await response.Content.ReadFromJsonAsync<NexusModsValidateResponse>();
                if (responseType is null || responseType.Key != apiKey)
                    return null;

                return responseType;
            }
        }

        public async Task<NexusModsModInfoResponse?> GetMod(string gameDomain, int modId, string apiKey)
        {
            var key = $"{gameDomain}:{modId}";
            if (await _distributedCache.GetStringAsync(key) is { } modJson)
            {
                return JsonSerializer.Deserialize<NexusModsModInfoResponse>(modJson, JsonSerializerOptions);
            }
            else
            {
                try
                {
                    await _lock.WaitAsync();
                    await _timeLimiter;

                    var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/games/{gameDomain}/mods/{modId}.json");
                    request.Headers.Add("apikey", apiKey);
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var httpClient = _httpClientFactory.CreateClient("NexusModsAPI");
                    var response = await httpClient.SendAsync(request);
                    modJson = await response.Content.ReadAsStringAsync();
                    var mod = JsonSerializer.Deserialize<NexusModsModInfoResponse>(modJson, JsonSerializerOptions);

                    _timeLimiter = ParseResponseLimits(response);
                    await _distributedCache.SetAsync(key, Encoding.UTF8.GetBytes(modJson), _expiration);
                    return response.IsSuccessStatusCode ? mod : null;
                }
                finally
                {
                    _lock.Release();
                }
            }
        }


        private static TimeLimiter ParseResponseLimits(HttpResponseMessage response)
        {
            // A 429 status code can also be served by nginx if the client sends more than 30 requests per second.
            // Nginx will however allow bursts over this for very short periods of time.
            var constraint = new CountByIntervalAwaitableConstraint(30, TimeSpan.FromSeconds(1));


            var hourlyLimit = int.Parse(response.Headers.GetValues("X-RL-Hourly-Limit").FirstOrDefault());
            var hourlyLimitConstraint = new CountByIntervalAwaitableConstraint(hourlyLimit, TimeSpan.FromHours(1));

            var hourlyRemaining = int.Parse(response.Headers.GetValues("X-RL-Hourly-Remaining").FirstOrDefault());
            var hourlyReset = DateTime.Parse(response.Headers.GetValues("X-RL-Hourly-Reset").FirstOrDefault());
            var hourlyTimeLeft = hourlyReset - DateTime.UtcNow;
            var hourlyRemainingConstraint = new CountByIntervalAwaitableConstraint(hourlyRemaining, hourlyTimeLeft);
            //var hourlyRemainingConstraint = new CountByIntervalAwaitableConstraint(hourlyRemaining, hourlyTimeLeft);


            var dailyLimit = int.Parse(response.Headers.GetValues("X-RL-Daily-Limit").FirstOrDefault());
            var dailyLimitConstraint = new CountByIntervalAwaitableConstraint(dailyLimit, TimeSpan.FromDays(1));

            var dailyRemaining = int.Parse(response.Headers.GetValues("X-RL-Daily-Remaining").FirstOrDefault());
            var dailyReset = DateTime.Parse(response.Headers.GetValues("X-RL-Daily-Reset").FirstOrDefault());
            var dailyTimeLeft = dailyReset - DateTime.UtcNow;
            var dailyRemainingConstraint = new CountByIntervalAwaitableConstraint(dailyRemaining, dailyTimeLeft);


            return TimeLimiter.Compose(constraint, hourlyLimitConstraint, hourlyRemainingConstraint, dailyLimitConstraint, dailyRemainingConstraint);
        }
    }
}