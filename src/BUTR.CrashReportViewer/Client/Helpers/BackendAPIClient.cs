using BUTR.CrashReportViewer.Shared.Models;
using BUTR.CrashReportViewer.Shared.Models.API;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace BUTR.CrashReportViewer.Client.Helpers
{
    public class BackendAPIClient
    {
        private static JsonSerializerOptions JsonSerializerOptions { get; } = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DemoUser _demoUser;

        public BackendAPIClient(IHttpClientFactory httpClientFactory, DemoUser demoUser)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _demoUser = demoUser ?? throw new ArgumentNullException(nameof(demoUser));
        }

        public async Task<string?> Authenticate(string apiKey)
        {
            if (apiKey.Equals("demo", StringComparison.OrdinalIgnoreCase))
            {
                _demoUser.Reset();
                return "demo";
            }

            var request = new HttpRequestMessage(HttpMethod.Get, "Authentication/authenticate");
            request.Headers.Add("apikey", apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var httpClient = _httpClientFactory.CreateClient("Backend");
            var response = await httpClient.SendAsync(request);
            return response.IsSuccessStatusCode ? (await response.Content.ReadFromJsonAsync<JwtTokenResponse>(JsonSerializerOptions))?.Token : null;
        }

        public async Task<bool> Validate(string token)
        {
            if (token.Equals("demo", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, "Authentication/validate");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var httpClient = _httpClientFactory.CreateClient("Backend");
            var response = await httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<ProfileModel?> GetProfile(string token)
        {
            if (token.Equals("demo", StringComparison.OrdinalIgnoreCase))
            {
                return _demoUser.Profile;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, "Authentication/profile");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var httpClient = _httpClientFactory.CreateClient("Backend");
            var response = await httpClient.SendAsync(request);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<ProfileModel>(JsonSerializerOptions) : null;
        }

        public async Task<ModModel[]?> GetMods(string token)
        {
            if (token.Equals("demo", StringComparison.OrdinalIgnoreCase))
            {
                return _demoUser.Mods.ToArray();
            }

            var request = new HttpRequestMessage(HttpMethod.Get, "Mods");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var httpClient = _httpClientFactory.CreateClient("Backend");
            var response = await httpClient.SendAsync(request);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<ModModel[]>(JsonSerializerOptions) : null;
        }

        public async Task<bool> RefreshMod(string token, string gameDomain, string modId)
        {
            if (token.Equals("demo", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"Mods/RefreshMod?gameDomain={gameDomain}&modId={modId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var httpClient = _httpClientFactory.CreateClient("Backend");
            var response = await httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> LinkMod(string token, string gameDomain, string modId)
        {
            if (token.Equals("demo", StringComparison.OrdinalIgnoreCase))
            {
                if (_demoUser.Mods.Find(m => m.GameDomain == gameDomain && m.ModId.ToString() == modId) is not { } mod && int.TryParse(modId, out var id))
                {
                    _demoUser.Mods.Add(new($"Demo Mod {id}", gameDomain, id));
                    return true;
                }

                return false;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"Mods/LinkMod?gameDomain={gameDomain}&modId={modId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var httpClient = _httpClientFactory.CreateClient("Backend");
            var response = await httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UnlinkMod(string token, string gameDomain, string modId)
        {
            if (token.Equals("demo", StringComparison.OrdinalIgnoreCase))
            {
                if (_demoUser.Mods.Find(m => m.GameDomain == gameDomain && m.ModId.ToString() == modId) is { } mod)
                    return _demoUser.Mods.Remove(mod);

                return false;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"Mods/UnlinkMod?gameDomain={gameDomain}&modId={modId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var httpClient = _httpClientFactory.CreateClient("Backend");
            var response = await httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<CrashReportModel[]?> GetCrashReports(string token)
        {
            if (token.Equals("demo", StringComparison.OrdinalIgnoreCase))
            {
                return _demoUser.CrashReports.ToArray();
            }

            var request = new HttpRequestMessage(HttpMethod.Get, "CrashReports");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var httpClient = _httpClientFactory.CreateClient("Backend");
            var response = await httpClient.SendAsync(request);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<CrashReportModel[]>(JsonSerializerOptions) : null;
        }
    }
}