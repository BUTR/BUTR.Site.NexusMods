using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.AspNetCore.Components.Authorization;

using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services
{
    public sealed class SimpleAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
    {
        private static readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, ApplicationRoles.Anonymous) }));
        private static readonly ClaimsPrincipal _user = new(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, ApplicationRoles.User) }, "NexusMods"));
        private static readonly ClaimsPrincipal _moderator = new(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, ApplicationRoles.Moderator) }, "NexusMods"));
        private static readonly ClaimsPrincipal _administrator = new(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, ApplicationRoles.Administrator) }, "NexusMods"));

        private readonly ITokenContainer _tokenContainer;
        private readonly IAuthenticationProvider _authenticationProvider;
        private Task<AuthenticationState> _task;

        public SimpleAuthenticationStateProvider(ITokenContainer tokenContainer, IAuthenticationProvider authenticationProvider)
        {
            _tokenContainer = tokenContainer ?? throw new ArgumentNullException(nameof(tokenContainer));
            _authenticationProvider = authenticationProvider ?? throw new ArgumentNullException(nameof(authenticationProvider));
            _tokenContainer.OnTokenChanged += ResetAuthenticationState;
            _task = GetAuthenticationStateInternalAsync();
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync() => _task;

        private async Task<AuthenticationState> GetAuthenticationStateInternalAsync()
        {
            if (await _authenticationProvider.ValidateAsync() is not { } profile)
                return new AuthenticationState(_anonymous);

            if (string.Equals(profile.Role, ApplicationRoles.Moderator, StringComparison.Ordinal))
                return new AuthenticationState(_moderator);
            
            if (string.Equals(profile.Role, ApplicationRoles.Administrator, StringComparison.Ordinal))
                return new AuthenticationState(_administrator);

            return new AuthenticationState(_user);
        }

        private void ResetAuthenticationState()
        {
            _task = GetAuthenticationStateInternalAsync();
            NotifyAuthenticationStateChanged(_task);
        }

        public void Dispose()
        {
            _tokenContainer.OnTokenChanged -= ResetAuthenticationState;
            _task.Dispose();
        }
    }
}