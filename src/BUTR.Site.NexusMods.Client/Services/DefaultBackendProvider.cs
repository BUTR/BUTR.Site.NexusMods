using BUTR.Site.NexusMods.Client.Models;
using BUTR.Site.NexusMods.Shared.Models;
using BUTR.Site.NexusMods.Shared.Models.API;

using Microsoft.Extensions.Options;

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services
{
    public sealed class DefaultAuthenticationProvider : IAuthenticationProvider, IAsyncDisposable
    {
        private readonly SimpleAuthenticationStateProvider _authenticationStateProvider;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenContainer _tokenContainer;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        private CancellationTokenSource _deauthorized = new();

        public DefaultAuthenticationProvider(SimpleAuthenticationStateProvider authenticationStateProvider, IHttpClientFactory httpClientFactory, ITokenContainer tokenContainer, IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _authenticationStateProvider = authenticationStateProvider ?? throw new ArgumentNullException(nameof(authenticationStateProvider));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _tokenContainer = tokenContainer ?? throw new ArgumentNullException(nameof(tokenContainer));
            _jsonSerializerOptions = jsonSerializerOptions.Value ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
        }

        public async Task<string?> AuthenticateAsync(string apiKey, string? type = null, CancellationToken ct = default)
        {
            await _tokenContainer.SetTokenTypeAsync(type, ct);

            if (type?.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            {
                _authenticationStateProvider.Notify();
                return "";
            }

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/Authentication/Authenticate{(string.IsNullOrEmpty(type) ? string.Empty : $"?type={type}")}");
                request.Headers.Add("apikey", apiKey);
                var httpClient = _httpClientFactory.CreateClient("Backend");
                var response = await httpClient.SendAsync(request, ct);
                if (response.IsSuccessStatusCode && await response.Content.ReadFromJsonAsync<JwtTokenResponse>(_jsonSerializerOptions, ct) is { } json)
                {
                    _deauthorized?.Cancel();
                    _deauthorized?.Dispose();
                    _deauthorized = new();

                    await _tokenContainer.SetTokenAsync(json.Token, ct);
                    _authenticationStateProvider.Notify();
                    return json.Token;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                await _tokenContainer.SetTokenTypeAsync(null, ct);
                return null;
            }
        }
        public async Task Deauthenticate(CancellationToken ct = default)
        {
            _deauthorized?.Cancel();
            _deauthorized?.Dispose();
            _deauthorized = null;
            await _tokenContainer.SetTokenAsync(null, ct);
            await _tokenContainer.SetTokenTypeAsync(null, ct);
            _authenticationStateProvider.Notify();
        }

        public ValueTask DisposeAsync()
        {
            _deauthorized?.Cancel();
            _deauthorized?.Dispose();
            _deauthorized = null;

            return ValueTask.CompletedTask;
        }
    }

    public sealed class DefaultBackendProvider : IProfileProvider, IRoleProvider, IModProvider, ICrashReportProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly LocalStorageCache _cache;
        private readonly ITokenContainer _tokenContainer;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public DefaultBackendProvider(IHttpClientFactory httpClientFactory, LocalStorageCache cache, ITokenContainer tokenContainer, IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _tokenContainer = tokenContainer ?? throw new ArgumentNullException(nameof(tokenContainer));
            _jsonSerializerOptions = jsonSerializerOptions.Value ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
        }

        public async Task<ProfileModel?> GetProfileAsync(CancellationToken ct = default)
        {
            async Task<(ProfileModel, CacheOptions)?> Factory()
            {
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/Authentication/Profile");
                    var httpClient = _httpClientFactory.CreateClient("Backend");
                    var response = await httpClient.SendAsync(request, ct);
                    if (!response.IsSuccessStatusCode || await response.Content.ReadFromJsonAsync<ProfileModel?>(_jsonSerializerOptions, ct) is not { } profile)
                        return null;

                    return new(profile, new CacheOptions
                    {
                        AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(5),
                        //ChangeToken = new CancellationChangeToken(_deauthorized.Token),
                    });
                }
                catch (Exception)
                {
                    return null;
                }
            }

            var tokenType = await _tokenContainer.GetTokenTypeAsync(ct);
            if (tokenType?.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            {
                return await DemoUser.GetProfile();
            }

            return await _cache.GetAsync("profile", Factory, ct);
        }

        public async Task<bool> SetRole(ulong userId, string role, CancellationToken ct = default)
        {
            var tokenType = await _tokenContainer.GetTokenTypeAsync(ct);
            if (tokenType?.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            {
                return true;
            }

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/Role")
                {
                    Content = new StringContent(JsonSerializer.Serialize(new { UserId = userId, Role = role }, _jsonSerializerOptions), Encoding.UTF8, "application/json")
                };
                var httpClient = _httpClientFactory.CreateClient("Backend");
                var response = await httpClient.SendAsync(request, ct);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> RemoveRole(ulong userId, CancellationToken ct = default)
        {
            var tokenType = await _tokenContainer.GetTokenTypeAsync(ct);
            if (tokenType?.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            {
                return true;
            }

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, "api/v1/Role")
                {
                    Content = new StringContent(JsonSerializer.Serialize(new { UserId = userId }, _jsonSerializerOptions), Encoding.UTF8, "application/json")
                };
                var httpClient = _httpClientFactory.CreateClient("Backend");
                var response = await httpClient.SendAsync(request, ct);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<PagingResponse<ModModel>?> GetMods(int page, CancellationToken ct = default)
        {
            var tokenType = await _tokenContainer.GetTokenTypeAsync(ct);
            if (tokenType?.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            {
                var mods = await DemoUser.GetMods();
                return new PagingResponse<ModModel>
                {
                    Items = mods,
                    Metadata = new PagingMetadata
                    {
                        PageSize = 10,
                        CurrentPage = 1,
                        TotalCount = mods.Count,
                        TotalPages = (int) Math.Ceiling((double) mods.Count / 10d),
                    }
                };
            }

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/Mods?page={page}&pageSize={10}");
                var httpClient = _httpClientFactory.CreateClient("Backend");
                var response = await httpClient.SendAsync(request, ct);
                return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<PagingResponse<ModModel>>(_jsonSerializerOptions, ct) : null;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<bool> LinkMod(string gameDomain, string modId, CancellationToken ct = default)
        {
            var tokenType = await _tokenContainer.GetTokenTypeAsync(ct);
            if (tokenType?.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            {
                var mods = await DemoUser.GetMods();
                if (mods.Find(m => m.GameDomain == gameDomain && m.ModId.ToString() == modId) is null && int.TryParse(modId, out var id))
                {
                    mods.Add(new($"Demo Mod {gameDomain} {id}", gameDomain, id));
                    return true;
                }

                return false;
            }

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/Mods/Link?gameDomain={gameDomain}&modId={modId}");
                var httpClient = _httpClientFactory.CreateClient("Backend");
                var response = await httpClient.SendAsync(request, ct);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> UnlinkMod(string gameDomain, string modId, CancellationToken ct = default)
        {
            var tokenType = await _tokenContainer.GetTokenTypeAsync(ct);
            if (tokenType?.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            {
                var mods = await DemoUser.GetMods();
                if (mods.Find(m => m.GameDomain == gameDomain && m.ModId.ToString() == modId) is { } mod)
                    return mods.Remove(mod);

                return false;
            }

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/Mods/Unlink?gameDomain={gameDomain}&modId={modId}");
                var httpClient = _httpClientFactory.CreateClient("Backend");
                var response = await httpClient.SendAsync(request, ct);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<PagingResponse<CrashReportModel>?> GetCrashReports(int page, CancellationToken ct = default)
        {
            var tokenType = await _tokenContainer.GetTokenTypeAsync(ct);
            if (tokenType?.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            {
                var crashReports = await DemoUser.GetCrashReports(_httpClientFactory);
                return new PagingResponse<CrashReportModel>
                {
                    Items = crashReports,
                    Metadata = new PagingMetadata
                    {
                        PageSize = 10,
                        CurrentPage = 1,
                        TotalCount = crashReports.Count,
                        TotalPages = (int) Math.Ceiling((double) crashReports.Count / 10d),
                    }
                };
            }

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/CrashReports?page={page}&pageSize={10}");
                var httpClient = _httpClientFactory.CreateClient("Backend");
                var response = await httpClient.SendAsync(request, ct);
                return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<PagingResponse<CrashReportModel>>(_jsonSerializerOptions, ct) : null;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<bool> UpdateCrashReport(CrashReportModel crashReport, CancellationToken ct = default)
        {
            var tokenType = await _tokenContainer.GetTokenTypeAsync(ct);
            if (tokenType?.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            {
                return true;
            }

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/CrashReports")
                {
                    Content = new StringContent(JsonSerializer.Serialize(crashReport, _jsonSerializerOptions), Encoding.UTF8, "application/json")
                };
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