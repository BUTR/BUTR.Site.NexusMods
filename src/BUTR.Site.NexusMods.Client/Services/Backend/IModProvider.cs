using BUTR.Site.NexusMods.ServerClient;

using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services
{
    public interface IModProvider
    {
        Task<ModModelPagingResponse?> GetMods(int page, CancellationToken ct = default);

        Task<bool> LinkMod(int nexusModsId, CancellationToken ct = default);
        Task<bool> UnlinkMod(int nexusModsId, CancellationToken ct = default);

        Task<ModNexusModsManualLinkModelPagingResponse?> GetManualLinks(int page, CancellationToken ct = default);
        Task<bool> ManualLink(string modId, int nexusModsId, CancellationToken ct = default);
        Task<bool> ManualUnlink(string modId, CancellationToken ct = default);

        Task<UserAllowedModsModelPagingResponse?> GetAllowUserMods(int page, CancellationToken ct = default);
        Task<bool> AllowUserMod(int userId, string modId, CancellationToken ct = default);
        Task<bool> DisallowUserMod(int userId, string modId, CancellationToken ct = default);
    }
}