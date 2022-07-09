using BUTR.Site.NexusMods.ServerClient;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services
{
    public sealed class DefaultRoleProvider : IRoleProvider
    {
        private readonly IUserClient _userClient;
        private readonly ITokenContainer _tokenContainer;

        public DefaultRoleProvider(IUserClient userClient, ITokenContainer tokenContainer)
        {
            _userClient = userClient ?? throw new ArgumentNullException(nameof(userClient));
            _tokenContainer = tokenContainer ?? throw new ArgumentNullException(nameof(tokenContainer));
        }

        public async Task<bool> SetRole(ulong userId, string role, CancellationToken ct = default)
        {
            var token = await _tokenContainer.GetTokenAsync(ct);
            if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
                return true;

            try
            {
                await _userClient.SetroleAsync(new SetRoleBody((int) userId, role), ct);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> RemoveRole(ulong userId, CancellationToken ct = default)
        {
            var token = await _tokenContainer.GetTokenAsync(ct);
            if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
                return true;

            try
            {
                await _userClient.RemoveroleAsync(new RemoveRoleBody((int) userId), ct);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}