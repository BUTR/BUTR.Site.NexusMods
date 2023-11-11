using BUTR.Site.NexusMods.Client.Models;
using BUTR.Site.NexusMods.ServerClient;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services;

public sealed class AuthenticationProvider
{
    private readonly IAuthenticationClient _authenticationClient;
    private readonly ITokenContainer _tokenContainer;

    public AuthenticationProvider(IAuthenticationClient client, ITokenContainer tokenContainer)
    {
        _authenticationClient = client ?? throw new ArgumentNullException(nameof(client));
        _tokenContainer = tokenContainer ?? throw new ArgumentNullException(nameof(tokenContainer));
    }

    public async Task<string?> AuthenticateAsync(string apiKey, string type, CancellationToken ct = default)
    {
        if (type.Equals("demo", StringComparison.OrdinalIgnoreCase))
        {
            await _tokenContainer.SetTokenAsync(new Token(type, string.Empty), ct);
            return string.Empty;
        }

        try
        {
            var response = (await _authenticationClient.AuthenticateAsync(apiKey, ct)).Value;

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
        if (await _tokenContainer.GetTokenAsync(ct) is not { } token)
            return null;

        if (token.Type.Equals("demo", StringComparison.OrdinalIgnoreCase))
            return await DemoUser.GetProfile();

        try
        {
            var response = (await _authenticationClient.ValidateAsync(ct)).Value;
            await _tokenContainer.RefreshTokenAsync(token with { Value = response.Token }, ct);
            return response.Profile;
        }
        catch (Exception)
        {
            return null;
        }
    }
}