using BUTR.Site.NexusMods.ServerClient;

using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services
{
    public interface IAuthenticationProvider
    {
        Task<string?> AuthenticateAsync(string apiKey, string? type = null, CancellationToken ct = default);
        Task DeauthenticateAsync(CancellationToken ct = default);
        Task<ProfileModel?> ValidateAsync(CancellationToken ct = default);
    }
}