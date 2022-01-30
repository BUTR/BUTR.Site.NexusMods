using BUTR.CrashReportViewer.Shared.Models;
using BUTR.NexusMods.Blazor.Core.Services;
using BUTR.NexusMods.Shared.Models.API;

using Microsoft.Extensions.Options;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.CrashReportViewer.Client.Helpers
{
    public class BackendAPIClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenContainer _tokenContainer;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private DemoUser? _demoUser;

        public BackendAPIClient(IHttpClientFactory httpClientFactory, ITokenContainer tokenContainer, IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _tokenContainer = tokenContainer ?? throw new ArgumentNullException(nameof(tokenContainer));
            _jsonSerializerOptions = jsonSerializerOptions.Value ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
        }

        public async Task<PagingResponse<ModModel>?> GetMods(string token, int page, CancellationToken ct = default)
        {
            var tokenType = await _tokenContainer.GetTokenTypeAsync(ct);
            if (tokenType?.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            {
                _demoUser ??= await DemoUser.CreateAsync(_httpClientFactory);
                return new PagingResponse<ModModel>
                {
                    Items = _demoUser.Mods,
                    Metadata = new PagingMetadata
                    {
                        PageSize = 10,
                        CurrentPage = 1,
                        TotalCount = _demoUser.Mods.Count,
                        TotalPages = (int) Math.Ceiling((double) _demoUser.Mods.Count / 10d),
                    }
                };
            }

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"Mods?page={page}&pageSize={10}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var httpClient = _httpClientFactory.CreateClient("Backend");
                var response = await httpClient.SendAsync(request, ct);
                return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<PagingResponse<ModModel>>(_jsonSerializerOptions, ct) : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> RefreshMod(string token, string gameDomain, string modId, CancellationToken ct = default)
        {
            var tokenType = await _tokenContainer.GetTokenTypeAsync(ct);
            if (tokenType?.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            {
                return true;
            }

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"Mods/Refresh?gameDomain={gameDomain}&modId={modId}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var httpClient = _httpClientFactory.CreateClient("Backend");
                var response = await httpClient.SendAsync(request, ct);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> LinkMod(string token, string gameDomain, string modId, CancellationToken ct = default)
        {
            var tokenType = await _tokenContainer.GetTokenTypeAsync(ct);
            if (tokenType?.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            {
                _demoUser ??= await DemoUser.CreateAsync(_httpClientFactory);
                if (_demoUser.Mods.Find(m => m.GameDomain == gameDomain && m.ModId.ToString() == modId) is null && int.TryParse(modId, out var id))
                {
                    _demoUser.Mods.Add(new($"Demo Mod {gameDomain} {id}", gameDomain, id));
                    return true;
                }

                return false;
            }

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"Mods/Link?gameDomain={gameDomain}&modId={modId}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var httpClient = _httpClientFactory.CreateClient("Backend");
                var response = await httpClient.SendAsync(request, ct);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UnlinkMod(string token, string gameDomain, string modId, CancellationToken ct = default)
        {
            var tokenType = await _tokenContainer.GetTokenTypeAsync(ct);
            if (tokenType?.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            {
                _demoUser ??= await DemoUser.CreateAsync(_httpClientFactory);
                if (_demoUser.Mods.Find(m => m.GameDomain == gameDomain && m.ModId.ToString() == modId) is { } mod)
                    return _demoUser.Mods.Remove(mod);

                return false;
            }

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"Mods/Unlink?gameDomain={gameDomain}&modId={modId}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var httpClient = _httpClientFactory.CreateClient("Backend");
                var response = await httpClient.SendAsync(request, ct);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<PagingResponse<CrashReportModel>?> GetCrashReports(string token, int page, CancellationToken ct = default)
        {
            var tokenType = await _tokenContainer.GetTokenTypeAsync(ct);
            if (tokenType?.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            {
                _demoUser ??= await DemoUser.CreateAsync(_httpClientFactory);
                return new PagingResponse<CrashReportModel>
                {
                    Items = _demoUser.CrashReports,
                    Metadata = new PagingMetadata
                    {
                        PageSize = 10,
                        CurrentPage = 1,
                        TotalCount = _demoUser.CrashReports.Count,
                        TotalPages = (int) Math.Ceiling((double) _demoUser.CrashReports.Count / 10d),
                    }
                };
            }

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"CrashReports?page={page}&pageSize={10}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var httpClient = _httpClientFactory.CreateClient("Backend");
                var response = await httpClient.SendAsync(request, ct);
                return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<PagingResponse<CrashReportModel>>(_jsonSerializerOptions, ct) : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> UpdateCrashReport(string token, CrashReportModel crashReport, CancellationToken ct = default)
        {
            var tokenType = await _tokenContainer.GetTokenTypeAsync(ct);
            if (tokenType?.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            {
                return true;
            }

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "CrashReports/Update");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Content = new StringContent(JsonSerializer.Serialize(crashReport, _jsonSerializerOptions), Encoding.UTF8, "application/json");
                var httpClient = _httpClientFactory.CreateClient("Backend");
                var response = await httpClient.SendAsync(request, ct);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}