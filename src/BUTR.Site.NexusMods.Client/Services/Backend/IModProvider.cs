using BUTR.Site.NexusMods.ServerClient;

using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services
{
    public interface IModProvider
    {
        Task<ModModelPagingDataAPIResponse?> GetMods(int page, int pageSize, CancellationToken ct = default);

        Task<bool> LinkMod(int nexusModsId, CancellationToken ct = default);
        Task<bool> UnlinkMod(int nexusModsId, CancellationToken ct = default);

        Task<ModNexusModsManualLinkModelPagingDataAPIResponse?> GetManualLinks(int page, int pageSize, CancellationToken ct = default);
        Task<bool> ManualLink(string modId, int nexusModsId, CancellationToken ct = default);
        Task<bool> ManualUnlink(string modId, CancellationToken ct = default);

        Task<UserAllowedModsModelPagingDataAPIResponse?> GetAllowUserMods(int page, int pageSize, CancellationToken ct = default);
        Task<bool> AllowUserMod(int userId, string modId, CancellationToken ct = default);
        Task<bool> DisallowUserMod(int userId, string modId, CancellationToken ct = default);
    }
}