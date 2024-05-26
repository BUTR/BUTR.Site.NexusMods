using BUTR.Site.NexusMods.ServerClient;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services;

public sealed class NexusModsModClientWithDemo : INexusModsModClient
{
    private readonly INexusModsModClient _implementation;
    private readonly ITokenContainer _tokenContainer;

    public NexusModsModClientWithDemo(IServiceProvider serviceProvider, ITokenContainer tokenContainer)
    {
        _implementation = Program.ConfigureClient(serviceProvider, (http, opt) => new NexusModsModClient(http, opt));
        _tokenContainer = tokenContainer;
    }

    public async Task<RawNexusModsModModelApiResultModel> GetRawAsync(int mod_id, CancellationToken ct = default)
    {
        return await _implementation.GetRawAsync(mod_id, ct);
    }

    public async Task<StringApiResultModel> AddModuleManualLinkAsync(int modId, string moduleId, CancellationToken ct = default)
    {
        return await _implementation.AddModuleManualLinkAsync(modId, moduleId, ct);
    }

    public async Task<StringApiResultModel> RemoveModuleManualLinkAsync(int modId, string moduleId, CancellationToken ct = default)
    {
        return await _implementation.RemoveModuleManualLinkAsync(modId, moduleId, ct);
    }

    public async Task<LinkedByStaffModuleNexusModsModsModelPagingDataApiResultModel> GetModuleManualLinkPaginatedAsync(PaginatedQuery body, CancellationToken ct = default)
    {
        return await _implementation.GetModuleManualLinkPaginatedAsync(body, ct);
    }
}