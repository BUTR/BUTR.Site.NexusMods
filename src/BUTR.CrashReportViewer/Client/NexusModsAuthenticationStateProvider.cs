using Blazored.LocalStorage;

using BUTR.CrashReportViewer.Shared.Helpers;
using BUTR.CrashReportViewer.Shared.Models.NexusModsAPI;

using Microsoft.AspNetCore.Components.Authorization;

using System.Security.Claims;
using System.Threading.Tasks;

namespace BUTR.CrashReportViewer.Client
{
    public class NexusModsAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly NexusModsAPIClient _nexusModsApiClient;

        public NexusModsAuthenticationStateProvider(ILocalStorageService localStorage, NexusModsAPIClient nexusModsApiClient)
        {
            _localStorage = localStorage;
            _nexusModsApiClient = nexusModsApiClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (!await _localStorage.ContainKeyAsync("user"))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var user = await _localStorage.GetItemAsync<NexusModsValidateResponse>("user");
            var validationResult = await _nexusModsApiClient.ValidateAPIKey(user.Key);
            if (validationResult is null)
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
            }, "NexusMods");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
    }
}