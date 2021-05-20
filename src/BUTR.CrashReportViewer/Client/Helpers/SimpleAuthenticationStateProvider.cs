using Blazored.LocalStorage;

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

            return new AuthenticationState(_authenticated);
        }
    }
}