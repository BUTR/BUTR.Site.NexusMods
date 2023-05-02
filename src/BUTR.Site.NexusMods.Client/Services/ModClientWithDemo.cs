using BUTR.Site.NexusMods.Client.Models;
using BUTR.Site.NexusMods.ServerClient;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services;

public sealed class ModClientWithDemo : IModClient
{
    private readonly IModClient _implementation;
    private readonly ITokenContainer _tokenContainer;

    public ModClientWithDemo(IServiceProvider serviceProvider, ITokenContainer tokenContainer)
    {
        _implementation = Program.ConfigureClient(serviceProvider, (http, opt) => new ModClient(http, opt));
        _tokenContainer = tokenContainer ?? throw new ArgumentNullException(nameof(tokenContainer));
    }
        
    public Task<RawModModelAPIResponse> RawGetAsync(string gameDomain, int modId, CancellationToken ct) => _implementation.RawGetAsync(gameDomain, modId, ct);

    public async Task<ModModelPagingDataAPIResponse> ModPaginatedAsync(PaginatedQuery? body, CancellationToken ct)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
        {
            var mods = await DemoUser.GetMods().ToListAsync(ct);
            return new ModModelPagingDataAPIResponse(new ModModelPagingData(mods, new PagingMetadata(1, (int) Math.Ceiling((double) mods.Count / (double) body.PageSize), body.PageSize, mods.Count)), string.Empty);
        }

        return await _implementation.ModPaginatedAsync(new PaginatedQuery(body.Page, body.PageSize, Array.Empty<Filtering>(), Array.Empty<Sorting>()), ct);
    }

    public Task<StringAPIResponse> ModUpdateAsync(ModQuery? body, CancellationToken ct) => _implementation.ModUpdateAsync(body, ct);

    public async Task<StringAPIResponse> ModLinkAsync(int? modId, CancellationToken ct)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
        {
            var mods = await DemoUser.GetMods().ToListAsync(ct);
            if (mods.Find(m => m.ModId == modId) is null)
            {
                mods.Add(new($"Demo Mod {modId}", modId ?? 0, ImmutableArray<int>.Empty, ImmutableArray<int>.Empty, ImmutableArray<string>.Empty, ImmutableArray<string>.Empty));
                return new StringAPIResponse("demo", string.Empty);
            }

            return new StringAPIResponse(null, "error");
        }

        return await _implementation.ModLinkAsync(modId, ct);
    }

    public async Task<StringAPIResponse> ModUnlinkAsync(int? modId, CancellationToken ct)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
        {
            var mods = await DemoUser.GetMods().ToListAsync(ct);
            if (mods.Find(m => m.ModId == modId) is { } mod)
            {
                mods.Remove(mod);
                return new StringAPIResponse("demo", string.Empty);
            }

            return new StringAPIResponse(null, "error");
        }

        return await _implementation.ModUnlinkAsync(modId, ct);
    }

    public async Task<StringAPIResponse> ModManualLinkAsync(string? modId, int? nexusModsId, CancellationToken ct)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new StringAPIResponse("demo", string.Empty);

        return await _implementation.ModManualLinkAsync(modId, nexusModsId, ct);
    }

    public async Task<StringAPIResponse> ModManualUnlinkAsync(string? modId, CancellationToken ct)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new StringAPIResponse("demo", string.Empty);

        return await _implementation.ModManualUnlinkAsync(modId, ct);
    }

    public async Task<ModNexusModsManualLinkModelPagingDataAPIResponse> ModManualLinkPaginatedAsync(PaginatedQuery? body, CancellationToken ct)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new ModNexusModsManualLinkModelPagingDataAPIResponse(new ModNexusModsManualLinkModelPagingData(new List<ModNexusModsManualLinkModel>(), new PagingMetadata(1, 1, body.PageSize, 1)), string.Empty);

        return await _implementation.ModManualLinkPaginatedAsync(new PaginatedQuery(body.Page, body.PageSize, Array.Empty<Filtering>(), Array.Empty<Sorting>()), ct);
    }

    public Task<StringAPIResponse> AllowUserAModuleIdAsync(int? userId, string? moduleId, CancellationToken ct) => _implementation.AllowUserAModuleIdAsync(userId, moduleId, ct);

    public Task<StringAPIResponse> DisallowUserAModuleIdAsync(int? userId, string? moduleId, CancellationToken ct) => _implementation.DisallowUserAModuleIdAsync(userId, moduleId, ct);

    public Task<UserAllowedModuleIdsModelPagingDataAPIResponse> AllowUserAModuleIdPaginatedAsync(PaginatedQuery? body, CancellationToken ct) => _implementation.AllowUserAModuleIdPaginatedAsync(body, ct);

    public async Task<StringAPIResponse> AllowUserAModAsync(int? userId, int? modId, CancellationToken ct)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new StringAPIResponse("demo", string.Empty);

        return await _implementation.AllowUserAModAsync(userId, modId, ct);
    }

    public async Task<StringAPIResponse> DisallowUserAModAsync(int? userId, int? modId, CancellationToken ct)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new StringAPIResponse("demo", string.Empty);

        return await _implementation.DisallowUserAModAsync(userId, modId, ct);
    }

    public async Task<UserAllowedModsModelPagingDataAPIResponse> AllowUserAModPaginatedAsync(PaginatedQuery? body, CancellationToken ct)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new UserAllowedModsModelPagingDataAPIResponse(new UserAllowedModsModelPagingData(new List<UserAllowedModsModel>(), new PagingMetadata(1, 1, body.PageSize, 1)), string.Empty);

        return await _implementation.AllowUserAModPaginatedAsync(new PaginatedQuery(body.Page, body.PageSize, Array.Empty<Filtering>(), Array.Empty<Sorting>()), ct);
    }
}