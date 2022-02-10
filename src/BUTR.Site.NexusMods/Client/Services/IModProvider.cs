using BUTR.Site.NexusMods.Shared.Models;
using BUTR.Site.NexusMods.Shared.Models.API;

using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services
{
    public interface IModProvider
    {
        Task<PagingResponse<ModModel>?> GetMods(int page, CancellationToken ct = default);
        Task<bool> LinkMod(string gameDomain, string modId, CancellationToken ct = default);
        Task<bool> UnlinkMod(string gameDomain, string modId, CancellationToken ct = default);
    }
}