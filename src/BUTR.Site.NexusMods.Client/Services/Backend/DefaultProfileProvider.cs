using BUTR.Site.NexusMods.Client.Models;
using BUTR.Site.NexusMods.ServerClient;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services
{
    public sealed class DefaultProfileProvider : IProfileProvider
    {
        private readonly IUserClient _client;
        private readonly StorageCache _cache;
        private readonly ITokenContainer _tokenContainer;

        public DefaultProfileProvider(IUserClient client, StorageCache cache, ITokenContainer tokenContainer)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _tokenContainer = tokenContainer ?? throw new ArgumentNullException(nameof(tokenContainer));
        }

        public async Task<ProfileModel?> GetProfileAsync(CancellationToken ct = default)
        {
            async Task<(ProfileModel, CacheOptions)?> Factory()
            {
                try
                {
                    var profile = (await _client.ProfileAsync(ct)).Data;
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

            var token = await _tokenContainer.GetTokenAsync(ct);
            if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            {
                return await DemoUser.GetProfile();
            }

            return await _cache.GetAsync("profile", Factory, ct);
        }
    }
}