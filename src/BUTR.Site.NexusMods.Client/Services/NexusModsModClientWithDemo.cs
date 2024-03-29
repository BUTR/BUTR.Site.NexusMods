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
        _tokenContainer = tokenContainer ?? throw new ArgumentNullException(nameof(tokenContainer));
    }

    public async Task<RawNexusModsModModelApiResultModel> RawAsync(string gameDomain, int modId, CancellationToken ct = default)
    {
        return await _implementation.RawAsync(gameDomain, modId, ct);
    }

    public async Task<StringApiResultModel> ToModuleManualLinkAsync(string? moduleId = null, int? nexusModsModId = null, CancellationToken ct = default)
    {
        return await _implementation.ToModuleManualLinkAsync(moduleId, nexusModsModId, ct);
    }

    public async Task<StringApiResultModel> ToModuleManualUnlinkAsync(string? moduleId = null, int? nexusModsModId = null, CancellationToken ct = default)
    {
        return await _implementation.ToModuleManualUnlinkAsync(moduleId, nexusModsModId, ct);
    }

    public async Task<NexusModsModToModuleModelPagingDataApiResultModel> ToModuleManualLinkPaginatedAsync(PaginatedQuery? body = null, CancellationToken ct = default)
    {
        return await _implementation.ToModuleManualLinkPaginatedAsync(body, ct);
    }

    public async Task<NexusModsModAvailableModelPagingDataApiResultModel> AvailablePaginatedAsync(PaginatedQuery? body = null, CancellationToken ct = default)
    {
        return await _implementation.AvailablePaginatedAsync(body, ct);
    }
}