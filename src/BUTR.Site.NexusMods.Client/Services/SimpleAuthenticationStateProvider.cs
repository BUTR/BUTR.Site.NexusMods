using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.AspNetCore.Components.Authorization;

using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services
{
    public sealed class SimpleAuthenticationStateProvider : AuthenticationStateProvider
    {
        private static readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());
        private static readonly ClaimsPrincipal _authenticated = new(new ClaimsIdentity(Array.Empty<Claim>(), "NexusMods"));
        private static readonly ClaimsPrincipal _administrator = new(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, ApplicationRoles.Administrator) }, "NexusMods"));

        private readonly IProfileProvider _profileProvider;

        public SimpleAuthenticationStateProvider(IProfileProvider profileProvider)
        {
            _profileProvider = profileProvider ?? throw new ArgumentNullException(nameof(profileProvider));
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (await _profileProvider.GetProfileAsync() is not { } profile)
                return new AuthenticationState(_anonymous);

            if (string.Equals(profile.Role, ApplicationRoles.Administrator, StringComparison.Ordinal))
                return new AuthenticationState(_administrator);

            return new AuthenticationState(_authenticated);
        }

        public void Notify()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}