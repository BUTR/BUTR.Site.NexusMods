using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services
{
    public interface IRoleProvider
    {
        Task<bool> SetRole(ulong userId, string role, CancellationToken ct = default);
        Task<bool> RemoveRole(ulong userId, CancellationToken ct = default);
    }
}