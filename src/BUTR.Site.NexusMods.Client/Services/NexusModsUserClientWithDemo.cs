using BUTR.Site.NexusMods.Client.Models;
using BUTR.Site.NexusMods.ServerClient;

using System;
using System.Collections.Generic;
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

    public async Task<ProfileModelApiResultModel> ProfileAsync(CancellationToken ct)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new ProfileModelApiResultModel(await DemoUser.GetProfile(), null!);

        async Task<(ProfileModelApiResultModel, CacheOptions)?> Factory()
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

        return await _cache.GetAsync("profile", Factory, ct) ?? new ProfileModelApiResultModel(await DemoUser.GetProfile(), null!);
    }

    public async Task<StringApiResultModel> SetRoleAsync(int? userId, string? role, CancellationToken ct)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new StringApiResultModel("demo", null!);

        return await _implementation.SetRoleAsync(userId, role, ct);
    }

    public async Task<StringApiResultModel> RemoveRoleAsync(int? userId, CancellationToken ct)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new StringApiResultModel("demo", null!);

        return await _implementation.RemoveRoleAsync(userId, ct);
    }

    public async Task<NexusModsModModelPagingDataApiResultModel> ToNexusModsModPaginatedAsync(PaginatedQuery? body = null, CancellationToken ct = default)
    {
        if (body is null)
            return await _implementation.ToNexusModsModPaginatedAsync(body, ct);

        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
        {
            var mods = await DemoUser.GetMods().ToListAsync(ct);
            return new NexusModsModModelPagingDataApiResultModel(new NexusModsModModelPagingData(PagingAdditionalMetadata.Empty, mods, new PagingMetadata(1, (int) Math.Ceiling((double) mods.Count / (double) body.PageSize), body.PageSize, mods.Count)), null!);
        }

        return await _implementation.ToNexusModsModPaginatedAsync(new PaginatedQuery(body.Page, body.PageSize, Array.Empty<Filtering>(), Array.Empty<Sorting>()), ct);
    }

    public async Task<StringApiResultModel> ToNexusModsModUpdateAsync(NexusModsUserToNexusModsModQuery? body = null, CancellationToken ct = default) =>
        await _implementation.ToNexusModsModUpdateAsync(body, ct);

    public async Task<StringApiResultModel> ToNexusModsModLinkAsync(int? nexusModsModId = null, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
        {
            var mods = await DemoUser.GetMods().ToListAsync(ct);
            if (mods.Find(m => m.NexusModsModId == nexusModsModId) is null)
            {
                mods.Add(new(nexusModsModId ?? 0, $"Demo Mod {nexusModsModId}", Array.Empty<int>(), Array.Empty<int>(), Array.Empty<string>(), Array.Empty<string>()));
                return new StringApiResultModel("demo", null!);
            }

            return new StringApiResultModel(null, null!);
        }

        return await _implementation.ToNexusModsModLinkAsync(nexusModsModId, ct);
    }

    public async Task<StringApiResultModel> ToNexusModsModUnlinkAsync(int? nexusModsModId = null, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
        {
            var mods = await DemoUser.GetMods().ToListAsync(ct);
            if (mods.Find(m => m.NexusModsModId == nexusModsModId) is { } mod)
            {
                mods.Remove(mod);
                return new StringApiResultModel("demo", null!);
            }

            return new StringApiResultModel(null!, null);
        }

        return await _implementation.ToNexusModsModUnlinkAsync(nexusModsModId, ct);
    }

    public async Task<StringApiResultModel> ToModuleManualLinkAsync(int? nexusModsUserId = null, string? moduleId = null, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new StringApiResultModel("demo", null!);

        return await _implementation.ToModuleManualLinkAsync(nexusModsUserId, moduleId, ct);
    }

    public async Task<StringApiResultModel> ToModuleManualUnlinkAsync(int? nexusModsUserId = null, string? moduleId = null, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new StringApiResultModel("demo", null!);

        return await _implementation.ToModuleManualUnlinkAsync(nexusModsUserId, moduleId, ct);
    }

    public async Task<NexusModsUserToModuleManualLinkModelPagingDataApiResultModel> ToModuleManualLinkPaginatedAsync(PaginatedQuery? body = null, CancellationToken ct = default)
    {
        if (body is null)
            return await _implementation.ToModuleManualLinkPaginatedAsync(body, ct);

        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new NexusModsUserToModuleManualLinkModelPagingDataApiResultModel(new NexusModsUserToModuleManualLinkModelPagingData(PagingAdditionalMetadata.Empty, new List<NexusModsUserToModuleManualLinkModel>(), new PagingMetadata(1, 1, body.PageSize, 1)), null!);

        return await _implementation.ToModuleManualLinkPaginatedAsync(new PaginatedQuery(body.Page, body.PageSize, Array.Empty<Filtering>(), Array.Empty<Sorting>()), ct);
    }

    public async Task<StringApiResultModel> ToNexusModsModManualLinkAsync(int? userId = null, int? nexusModsModId = null, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new StringApiResultModel("demo", null!);

        return await _implementation.ToNexusModsModManualLinkAsync(userId, nexusModsModId, ct);
    }

    public async Task<StringApiResultModel> ToNexusModsModManualUnlinkAsync(int? userId = null, int? nexusModsModId = null, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new StringApiResultModel("demo", null!);

        return await _implementation.ToNexusModsModManualUnlinkAsync(userId, nexusModsModId, ct);
    }

    public async Task<NexusModsUserToNexusModsModManualLinkModelPagingDataApiResultModel> ToNexusModsModManualLinkPaginatedAsync(PaginatedQuery? body = null, CancellationToken ct = default)
    {
        if (body is null)
            return await _implementation.ToNexusModsModManualLinkPaginatedAsync(body, ct);

        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new NexusModsUserToNexusModsModManualLinkModelPagingDataApiResultModel(new NexusModsUserToNexusModsModManualLinkModelPagingData(PagingAdditionalMetadata.Empty, new List<NexusModsUserToNexusModsModManualLinkModel>(), new PagingMetadata(1, 1, body.PageSize, 1)), null!);

        return await _implementation.ToNexusModsModManualLinkPaginatedAsync(body, ct);
    }
}