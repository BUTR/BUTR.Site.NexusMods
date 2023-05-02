using BUTR.Site.NexusMods.Client.Models;
using BUTR.Site.NexusMods.ServerClient;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services;

public sealed class UserClientWithDemo : IUserClient
{
    private readonly IUserClient _implementation;
    private readonly ITokenContainer _tokenContainer;
    private readonly StorageCache _cache;

    public UserClientWithDemo(IServiceProvider serviceProvider, ITokenContainer tokenContainer, StorageCache cache)
    {
        _implementation = Program.ConfigureClient(serviceProvider, (http, opt) => new UserClient(http, opt));
        _tokenContainer = tokenContainer ?? throw new ArgumentNullException(nameof(tokenContainer));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<ProfileModelAPIResponse> ProfileAsync(CancellationToken ct)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new ProfileModelAPIResponse(await DemoUser.GetProfile(), string.Empty);

        async Task<(ProfileModelAPIResponse, CacheOptions)?> Factory()
        {
            try
            {
                var profile = (await _implementation.ProfileAsync(ct)).Data;
                return new(new(profile, string.Empty), new CacheOptions
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

        return await _cache.GetAsync("profile", Factory, ct) ?? new ProfileModelAPIResponse(await DemoUser.GetProfile(), "error");
    }

    public async Task<StringAPIResponse> SetRoleAsync(int? userId, string? role, CancellationToken ct)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new StringAPIResponse("demo", string.Empty);

        return await _implementation.SetRoleAsync(userId, role, ct);
    }

    public async Task<StringAPIResponse> RemoveRoleAsync(int? userId, CancellationToken ct)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new StringAPIResponse("demo", string.Empty);

        return await _implementation.RemoveRoleAsync(userId, ct);
    }
}