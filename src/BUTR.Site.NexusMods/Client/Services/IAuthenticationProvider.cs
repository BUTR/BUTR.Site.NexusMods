using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services
{
    public interface IAuthenticationProvider
    {
        Task<string?> AuthenticateAsync(string apiKey, string? type = null, CancellationToken ct = default);
        Task Deauthenticate(CancellationToken ct = default);
    }
}