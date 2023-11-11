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

    public async Task<ProfileModelAPIResponseActionResult> ProfileAsync(CancellationToken ct)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new ProfileModelAPIResponseActionResult(await DemoUser.GetProfile(), null!);

        async Task<(ProfileModelAPIResponseActionResult, CacheOptions)?> Factory()
        {
            try
            {
                var profile = (await _implementation.ProfileAsync(ct)).Value;
                return new(new(profile, null!), new CacheOptions
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

        return await _cache.GetAsync("profile", Factory, ct) ?? new ProfileModelAPIResponseActionResult(await DemoUser.GetProfile(), new(null, null, null, detail: "error", null));
    }

    public async Task<StringAPIResponseActionResult> SetRoleAsync(int? userId, string? role, CancellationToken ct)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new StringAPIResponseActionResult("demo", null!);

        return await _implementation.SetRoleAsync(userId, role, ct);
    }

    public async Task<StringAPIResponseActionResult> RemoveRoleAsync(int? userId, CancellationToken ct)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new StringAPIResponseActionResult("demo", null!);

        return await _implementation.RemoveRoleAsync(userId, ct);
    }

    public async Task<NexusModsModModelPagingDataAPIResponseActionResult> ToNexusModsModPaginatedAsync(PaginatedQuery? body = null, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
        {
            var mods = await DemoUser.GetMods().ToListAsync(ct);
            return new NexusModsModModelPagingDataAPIResponseActionResult(new NexusModsModModelPagingData(PagingAdditionalMetadata.Empty, mods, new PagingMetadata(1, (int) Math.Ceiling((double) mods.Count / (double) body.PageSize), body.PageSize, mods.Count)), null!);
        }

        return await _implementation.ToNexusModsModPaginatedAsync(new PaginatedQuery(body.Page, body.PageSize, Array.Empty<Filtering>(), Array.Empty<Sorting>()), ct);
    }

    public async Task<StringAPIResponseActionResult> ToNexusModsModUpdateAsync(NexusModsUserToNexusModsModQuery? body = null, CancellationToken ct = default) =>
        await _implementation.ToNexusModsModUpdateAsync(body, ct);

    public async Task<StringAPIResponseActionResult> ToNexusModsModLinkAsync(int? nexusModsModId = null, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
        {
            var mods = await DemoUser.GetMods().ToListAsync(ct);
            if (mods.Find(m => m.NexusModsModId == nexusModsModId) is null)
            {
                mods.Add(new(nexusModsModId ?? 0, $"Demo Mod {nexusModsModId}", ImmutableArray<int>.Empty, ImmutableArray<int>.Empty, ImmutableArray<string>.Empty, ImmutableArray<string>.Empty));
                return new StringAPIResponseActionResult("demo", null!);
            }

            return new StringAPIResponseActionResult(null, new(null, null, null, detail: "error", null));
        }

        return await _implementation.ToNexusModsModLinkAsync(nexusModsModId, ct);
    }

    public async Task<StringAPIResponseActionResult> ToNexusModsModUnlinkAsync(int? nexusModsModId = null, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
        {
            var mods = await DemoUser.GetMods().ToListAsync(ct);
            if (mods.Find(m => m.NexusModsModId == nexusModsModId) is { } mod)
            {
                mods.Remove(mod);
                return new StringAPIResponseActionResult("demo", null!);
            }

            return new StringAPIResponseActionResult(null, new(null, null, null, detail: "error", null));
        }

        return await _implementation.ToNexusModsModUnlinkAsync(nexusModsModId, ct);
    }

    public async Task<StringAPIResponseActionResult> ToModuleManualLinkAsync(int? nexusModsUserId = null, string? moduleId = null, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new StringAPIResponseActionResult("demo", null!);

        return await _implementation.ToModuleManualLinkAsync(nexusModsUserId, moduleId, ct);
    }

    public async Task<StringAPIResponseActionResult> ToModuleManualUnlinkAsync(int? nexusModsUserId = null, string? moduleId = null, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new StringAPIResponseActionResult("demo", null!);

        return await _implementation.ToModuleManualUnlinkAsync(nexusModsUserId, moduleId, ct);
    }

    public async Task<NexusModsUserToModuleManualLinkModelPagingDataAPIResponseActionResult> ToModuleManualLinkPaginatedAsync(PaginatedQuery? body = null, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new NexusModsUserToModuleManualLinkModelPagingDataAPIResponseActionResult(new NexusModsUserToModuleManualLinkModelPagingData(PagingAdditionalMetadata.Empty, new List<NexusModsUserToModuleManualLinkModel>(), new PagingMetadata(1, 1, body.PageSize, 1)), null!);

        return await _implementation.ToModuleManualLinkPaginatedAsync(new PaginatedQuery(body.Page, body.PageSize, Array.Empty<Filtering>(), Array.Empty<Sorting>()), ct);
    }

    public async Task<StringAPIResponseActionResult> ToNexusModsModManualLinkAsync(int? userId = null, int? nexusModsModId = null, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new StringAPIResponseActionResult("demo", null!);

        return await _implementation.ToNexusModsModManualLinkAsync(userId, nexusModsModId, ct);
    }

    public async Task<StringAPIResponseActionResult> ToNexusModsModManualUnlinkAsync(int? userId = null, int? nexusModsModId = null, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new StringAPIResponseActionResult("demo", null!);

        return await _implementation.ToNexusModsModManualUnlinkAsync(userId, nexusModsModId, ct);
    }

    public async Task<NexusModsUserToNexusModsModManualLinkModelPagingDataAPIResponseActionResult> ToNexusModsModManualLinkPaginatedAsync(PaginatedQuery? body = null, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new NexusModsUserToNexusModsModManualLinkModelPagingDataAPIResponseActionResult(new NexusModsUserToNexusModsModManualLinkModelPagingData(PagingAdditionalMetadata.Empty, new List<NexusModsUserToNexusModsModManualLinkModel>(), new PagingMetadata(1, 1, body.PageSize, 1)), null!);

        return await _implementation.ToNexusModsModManualLinkPaginatedAsync(body, ct);
    }
}