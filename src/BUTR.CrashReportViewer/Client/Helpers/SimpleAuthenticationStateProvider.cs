using Blazored.LocalStorage;

using BUTR.CrashReportViewer.Shared.Helpers;

using Microsoft.AspNetCore.Components.Authorization;

using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BUTR.CrashReportViewer.Client.Helpers
{
    public class SimpleAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());
        private readonly ClaimsPrincipal _authenticated = new(new ClaimsIdentity(Array.Empty<Claim>(), "NexusMods"));
        private readonly ClaimsPrincipal _administrator = new(new ClaimsIdentity(new [] { new Claim(ClaimTypes.Role, ApplicationRoles.Administrator) }, "Standard"));
        private readonly ILocalStorageService _localStorage;
        private readonly BackendAPIClient _backendApiClient;

        public SimpleAuthenticationStateProvider(ILocalStorageService localStorage, BackendAPIClient backendApiClient)
        {
            _localStorage = localStorage;
            _backendApiClient = backendApiClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (!await _localStorage.ContainKeyAsync("token"))
                return new AuthenticationState(_anonymous);

            var token = await _localStorage.GetItemAsStringAsync("token");
            if (!await _backendApiClient.Validate(token))
                return new AuthenticationState(_anonymous);

            if (await _backendApiClient.GetProfile(token) is { } profile && profile.UserId == -1)
                return new AuthenticationState(_administrator);

            return new AuthenticationState(_authenticated);
        }
    }
}