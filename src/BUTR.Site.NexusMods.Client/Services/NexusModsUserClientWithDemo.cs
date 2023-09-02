using BUTR.Site.NexusMods.Client.Models;
using BUTR.Site.NexusMods.ServerClient;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services;

public sealed class NexusModsUserClientWithDemo : INexusModsUserClient
{
    private readonly INexusModsUserClient _implementation;
    private readonly ITokenContainer _tokenContainer;
    private readonly StorageCache _cache;

    public NexusModsUserClientWithDemo(IServiceProvider serviceProvider, ITokenContainer tokenContainer, StorageCache cache)
    {
        _implementation = Program.ConfigureClient(serviceProvider, (http, opt) => new NexusModsUserClient(http, opt));
        _tokenContainer = tokenContainer ?? throw new ArgumentNullException(nameof(tokenContainer));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<ProfileModelAPIResponse> ProfileAsync(CancellationToken ct)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new ProfileModelAPIResponse(await DemoUser.GetProfile(), string.Empty);

        async Task<(ProfileModelAPIResponse, CacheOptions)?> Factory()
        {
            try
            {
                var profile = (await _implementation.ProfileAsync(ct)).Data;
                return new(new(profile, string.Empty), new CacheOptions
                {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(5),
                    //ChangeToken = new CancellationChangeToken(_deauthorized.Token),
                });
            }
            catch (Exception)
            {
                return null;
            }
        }

        return await _cache.GetAsync("profile", Factory, ct) ?? new ProfileModelAPIResponse(await DemoUser.GetProfile(), "error");
    }

    public async Task<StringAPIResponse> SetRoleAsync(int? userId, string? role, CancellationToken ct)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new StringAPIResponse("demo", string.Empty);

        return await _implementation.SetRoleAsync(userId, role, ct);
    }

    public async Task<StringAPIResponse> RemoveRoleAsync(int? userId, CancellationToken ct)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new StringAPIResponse("demo", string.Empty);

        return await _implementation.RemoveRoleAsync(userId, ct);
    }

    public async Task<NexusModsModModelPagingDataAPIResponse> ToNexusModsModPaginatedAsync(PaginatedQuery? body = null, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
        {
            var mods = await DemoUser.GetMods().ToListAsync(ct);
            return new NexusModsModModelPagingDataAPIResponse(new NexusModsModModelPagingData(PagingAdditionalMetadata.Empty, mods, new PagingMetadata(1, (int) Math.Ceiling((double) mods.Count / (double) body.PageSize), body.PageSize, mods.Count)), string.Empty);
        }

        return await _implementation.ToNexusModsModPaginatedAsync(new PaginatedQuery(body.Page, body.PageSize, Array.Empty<Filtering>(), Array.Empty<Sorting>()), ct);
    }

    public async Task<StringAPIResponse> ToNexusModsModUpdateAsync(NexusModsUserToNexusModsModQuery? body = null, CancellationToken ct = default) => await _implementation.ToNexusModsModUpdateAsync(body, ct);

    public async Task<StringAPIResponse> ToNexusModsModLinkAsync(int? nexusModsModId = null, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
        {
            var mods = await DemoUser.GetMods().ToListAsync(ct);
            if (mods.Find(m => m.NexusModsModId == nexusModsModId) is null)
            {
                mods.Add(new(nexusModsModId ?? 0, $"Demo Mod {nexusModsModId}", ImmutableArray<int>.Empty, ImmutableArray<int>.Empty, ImmutableArray<string>.Empty, ImmutableArray<string>.Empty));
                return new StringAPIResponse("demo", string.Empty);
            }

            return new StringAPIResponse(null, "error");
        }

        return await _implementation.ToNexusModsModLinkAsync(nexusModsModId, ct);
    }

    public async Task<StringAPIResponse> ToNexusModsModUnlinkAsync(int? nexusModsModId = null, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
        {
            var mods = await DemoUser.GetMods().ToListAsync(ct);
            if (mods.Find(m => m.NexusModsModId == nexusModsModId) is { } mod)
            {
                mods.Remove(mod);
                return new StringAPIResponse("demo", string.Empty);
            }

            return new StringAPIResponse(null, "error");
        }

        return await _implementation.ToNexusModsModUnlinkAsync(nexusModsModId, ct);
    }

    public async Task<StringAPIResponse> ToModuleManualLinkAsync(int? nexusModsUserId = null, string? moduleId = null, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new StringAPIResponse("demo", string.Empty);

        return await _implementation.ToModuleManualLinkAsync(nexusModsUserId, moduleId, ct);
    }

    public async Task<StringAPIResponse> ToModuleManualUnlinkAsync(int? nexusModsUserId = null, string? moduleId = null, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new StringAPIResponse("demo", string.Empty);

        return await _implementation.ToModuleManualUnlinkAsync(nexusModsUserId, moduleId, ct);
    }

    public async Task<NexusModsUserToModuleManualLinkModelPagingDataAPIResponse> ToModuleManualLinkPaginatedAsync(PaginatedQuery? body = null, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new NexusModsUserToModuleManualLinkModelPagingDataAPIResponse(new NexusModsUserToModuleManualLinkModelPagingData(PagingAdditionalMetadata.Empty, new List<NexusModsUserToModuleManualLinkModel>(), new PagingMetadata(1, 1, body.PageSize, 1)), string.Empty);

        return await _implementation.ToModuleManualLinkPaginatedAsync(new PaginatedQuery(body.Page, body.PageSize, Array.Empty<Filtering>(), Array.Empty<Sorting>()), ct);
    }

    public async Task<StringAPIResponse> ToNexusModsModManualLinkAsync(int? userId = null, int? nexusModsModId = null, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new StringAPIResponse("demo", string.Empty);

        return await _implementation.ToNexusModsModManualLinkAsync(userId, nexusModsModId, ct);
    }

    public async Task<StringAPIResponse> ToNexusModsModManualUnlinkAsync(int? userId = null, int? nexusModsModId = null, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new StringAPIResponse("demo", string.Empty);

        return await _implementation.ToNexusModsModManualUnlinkAsync(userId, nexusModsModId, ct);
    }

    public async Task<NexusModsUserToNexusModsModManualLinkModelPagingDataAPIResponse> ToNexusModsModManualLinkPaginatedAsync(PaginatedQuery? body = null, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new NexusModsUserToNexusModsModManualLinkModelPagingDataAPIResponse(new NexusModsUserToNexusModsModManualLinkModelPagingData(PagingAdditionalMetadata.Empty, new List<NexusModsUserToNexusModsModManualLinkModel>(), new PagingMetadata(1, 1, body.PageSize, 1)), string.Empty);

        return await _implementation.ToNexusModsModManualLinkPaginatedAsync(body, ct);
    }
}