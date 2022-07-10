using BUTR.Site.NexusMods.ServerClient;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services
{
    public sealed class DefaultAuthenticationProvider : IAuthenticationProvider
    {
        private readonly IAuthenticationClient _authenticationClient;
        private readonly ITokenContainer _tokenContainer;
        private readonly IProfileProvider _profileProvider;

        public DefaultAuthenticationProvider(IAuthenticationClient client, ITokenContainer tokenContainer, IProfileProvider profileProvider)
        {
            _authenticationClient = client ?? throw new ArgumentNullException(nameof(client));
            _tokenContainer = tokenContainer ?? throw new ArgumentNullException(nameof(tokenContainer));
            _profileProvider = profileProvider ?? throw new ArgumentNullException(nameof(profileProvider));
        }

        public async Task<string?> AuthenticateAsync(string apiKey, string type, CancellationToken ct = default)
        {
            if (type?.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            {
                await _tokenContainer.SetTokenAsync(new Token(type, string.Empty), ct);
                return string.Empty;
            }

            try
            {
                var response = await _authenticationClient.AuthenticateAsync(apiKey, ct);

                await _tokenContainer.SetTokenAsync(new Token(type, response.Token), ct);

                return response.Token;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task DeauthenticateAsync(CancellationToken ct = default)
        {
            await _tokenContainer.SetTokenAsync(null, ct);
        }

        public async Task<ProfileModel?> ValidateAsync(CancellationToken ct = default)
        {
            var token = await _tokenContainer.GetTokenAsync(ct);
            if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
                return await _profileProvider.GetProfileAsync(ct);

            try
            {
                var response = await _authenticationClient.ValidateAsync(ct);
                return response.Profile;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}