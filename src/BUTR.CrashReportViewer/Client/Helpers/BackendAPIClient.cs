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

        public BackendAPIClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<string?> Authenticate(string apiKey)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "Authentication/authenticate");
            request.Headers.Add("apikey", apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var httpClient = _httpClientFactory.CreateClient("Backend");
            var response = await httpClient.SendAsync(request);
            return response.IsSuccessStatusCode ? (await response.Content.ReadFromJsonAsync<JwtTokenResponse>(JsonSerializerOptions))?.Token : null;
        }

        public async Task<bool> Validate(string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "Authentication/validate");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var httpClient = _httpClientFactory.CreateClient("Backend");
            var response = await httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<ProfileModel?> GetProfile(string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "Authentication/profile");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var httpClient = _httpClientFactory.CreateClient("Backend");
            var response = await httpClient.SendAsync(request);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<ProfileModel>(JsonSerializerOptions) : null;
        }

        public async Task<ModModel[]?> GetMods(string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "Mods");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var httpClient = _httpClientFactory.CreateClient("Backend");
            var response = await httpClient.SendAsync(request);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<ModModel[]>(JsonSerializerOptions) : null;
        }

        public async Task<bool> LinkMod(string token, string gameDomain, string modId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"Mods/LinkMod?gameDomain={gameDomain}&modId={modId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var httpClient = _httpClientFactory.CreateClient("Backend");
            var response = await httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UnlinkMod(string token, string gameDomain, string modId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"Mods/UnlinkMod?gameDomain={gameDomain}&modId={modId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var httpClient = _httpClientFactory.CreateClient("Backend");
            var response = await httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<CrashReportModel[]?> GetCrashReports(string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "CrashReports");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var httpClient = _httpClientFactory.CreateClient("Backend");
            var response = await httpClient.SendAsync(request);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<CrashReportModel[]>(JsonSerializerOptions) : null;
        }
    }
}