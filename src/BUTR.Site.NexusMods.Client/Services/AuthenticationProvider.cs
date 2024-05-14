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
        _authenticationClient = client;
        _tokenContainer = tokenContainer;
    }

    public async Task<string?> AuthenticateWithApiKeyAsync(string apiKey, string type, CancellationToken ct = default)
    {
        if (type.Equals("demo", StringComparison.OrdinalIgnoreCase))
        {
            await _tokenContainer.SetTokenAsync(new Token(type, string.Empty), ct);
            return string.Empty;
        }

        try
        {
            var response = await _authenticationClient.AuthenticateWithApiKeyAsync(apiKey, ct);
            if (response.Value is null) return null;

            await _tokenContainer.SetTokenAsync(new Token(type, response.Value.Token), ct);

            return response.Value.Token;
        }
        catch (Exception e)
        {
            return null;
        }
    }
    public async Task<string?> AuthenticateWithOAuth2Async(string code, Guid state, string type, CancellationToken ct = default)
    {
        if (type.Equals("demo", StringComparison.OrdinalIgnoreCase))
        {
            await _tokenContainer.SetTokenAsync(new Token(type, string.Empty), ct);
            return string.Empty;
        }

        try
        {
            var response = await _authenticationClient.AuthenticateWithOAuth2Async(code, state, ct);
            if (response.Value is null) return null;

            await _tokenContainer.SetTokenAsync(new Token(type, response.Value.Token), ct);

            return response.Value.Token;
        }
        catch (Exception e)
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
            var response = await _authenticationClient.ValidateAsync(ct);
            if (response.Value is null) return null;

            await _tokenContainer.RefreshTokenAsync(token with { Value = response.Value.Token }, ct);
            return response.Value.Profile;
        }
        catch (Exception)
        {
            return null;
        }
    }
}