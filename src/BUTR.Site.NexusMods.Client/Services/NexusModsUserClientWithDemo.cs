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
        _tokenContainer = tokenContainer;
        _cache = cache;
    }

    public async Task<ProfileModelApiResultModel> GetProfileAsync(CancellationToken ct)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new ProfileModelApiResultModel(await DemoUser.GetProfile(), null!);

        async Task<(ProfileModelApiResultModel, CacheOptions)?> Factory()
        {
            try
            {
                var profile = (await _implementation.GetProfileAsync(ct)).Value;
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

    public async Task<StringApiResultModel> SetRoleAsync(string role, int? userId = null, string? username = null, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new StringApiResultModel("demo", null!);

        return await _implementation.SetRoleAsync(role, userId, username, ct);
    }

    public async Task<StringApiResultModel> RemoveRoleAsync(int? userId = null, string? username = null, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new StringApiResultModel("demo", null!);

        return await _implementation.RemoveRoleAsync(userId, username, ct);
    }

    public async Task<UserLinkedNexusModsModModelPagingDataApiResultModel> GetNexusModsModsPaginatedAsync(PaginatedQuery body, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
        {
            var mods = await DemoUser.GetMods().ToListAsync(ct);
            return new UserLinkedNexusModsModModelPagingDataApiResultModel(new UserLinkedNexusModsModModelPagingData(PagingAdditionalMetadata.Empty, mods, new PagingMetadata(1, (int) Math.Ceiling((double) mods.Count / (double) body.PageSize), body.PageSize, mods.Count)), null!);
        }

        return await _implementation.GetNexusModsModsPaginatedAsync(body, ct);
    }

    public async Task<StringApiResultModel> UpdateNexusModsModLinkAsync(int modId, int? userId = null, string? username = null, CancellationToken ct = default) =>
        await _implementation.UpdateNexusModsModLinkAsync(modId, userId, username, ct);

    public async Task<StringApiResultModel> AddNexusModsModLinkAsync(int modId, int? userId = null, string? username = null, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
        {
            var mods = await DemoUser.GetMods().ToListAsync(ct);
            if (mods.Find(m => m.NexusModsModId == modId) is null)
            {
                mods.Add(new(modId, $"Demo Mod {modId}", Array.Empty<int>(), Array.Empty<int>(), Array.Empty<int>(), Array.Empty<string>(), Array.Empty<string>()));
                return new StringApiResultModel("demo", null!);
            }

            return new StringApiResultModel(null, null!);
        }

        return await _implementation.AddNexusModsModLinkAsync(modId, userId, username, ct);
    }

    public async Task<StringApiResultModel> RemoveNexusModsModLinkAsync(int modId, int? userId = null, string? username = null, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
        {
            var mods = await DemoUser.GetMods().ToListAsync(ct);
            if (mods.Find(m => m.NexusModsModId == modId) is { } mod)
            {
                mods.Remove(mod);
                return new StringApiResultModel("demo", null!);
            }

            return new StringApiResultModel(null!, null);
        }

        return await _implementation.RemoveNexusModsModLinkAsync(modId, userId, username, ct);
    }

    public async Task<UserLinkedSteamWorkshopModModelPagingDataApiResultModel> GetSteamWorkshopModsPaginatedAsync(PaginatedQuery body, CancellationToken ct = default)
    {
        return await _implementation.GetSteamWorkshopModsPaginatedAsync(body, ct);
    }

    public async Task<UserAvailableSteamWorkshopModModelPagingDataApiResultModel> GetSteamWorkshopModsPaginateAvailabledAsync(PaginatedQuery body, CancellationToken ct = default)
    {
        return await _implementation.GetSteamWorkshopModsPaginateAvailabledAsync(body, ct);
    }

    public async Task<StringApiResultModel> AddSteamWorkshopModLinkImportAllAsync(int? userId = null, string? username = null, CancellationToken ct = default)
    {
        return await _implementation.AddSteamWorkshopModLinkImportAllAsync(userId, username, ct);
    }

    public async Task<StringApiResultModel> AddSteamWorkshopModLinkAsync(string modId, int? userId = null, string? username = null, CancellationToken ct = default)
    {
        return await _implementation.AddSteamWorkshopModLinkAsync(modId, userId, username, ct);
    }

    public async Task<StringApiResultModel> UpdateSteamWorkshopModLinkAsync(string modId, int? userId = null, string? username = null, CancellationToken ct = default)
    {
        return await _implementation.UpdateSteamWorkshopModLinkAsync(modId, userId, username, ct);
    }

    public async Task<StringApiResultModel> RemoveSteamWorkshopModLinkAsync(string modId, int? userId = null, string? username = null, CancellationToken ct = default)
    {
        return await _implementation.RemoveSteamWorkshopModLinkAsync(modId, userId, username, ct);
    }

    public async Task<StringApiResultModel> AddModuleManualLinkAsync(string moduleId, int? userId = null, string? username = null, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new StringApiResultModel("demo", null!);

        return await _implementation.AddModuleManualLinkAsync(moduleId, userId, username, ct);
    }

    public async Task<StringApiResultModel> RemoveModuleManualLinkAsync(string moduleId, int? userId = null, string? username = null, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new StringApiResultModel("demo", null!);

        return await _implementation.RemoveModuleManualLinkAsync(moduleId, userId, username, ct);
    }

    public async Task<UserManuallyLinkedModuleModelPagingDataApiResultModel> GetModuleManualLinkPaginatedAsync(PaginatedQuery body, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new UserManuallyLinkedModuleModelPagingDataApiResultModel(new UserManuallyLinkedModuleModelPagingData(PagingAdditionalMetadata.Empty, new List<UserManuallyLinkedModuleModel>(), new PagingMetadata(1, 1, body.PageSize, 1)), null!);

        return await _implementation.GetModuleManualLinkPaginatedAsync(body, ct);
    }

    public async Task<StringApiResultModel> AddNexusModsModManualLinkAsync(int modId, int? userId = null, string? username = null, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new StringApiResultModel("demo", null!);

        return await _implementation.AddNexusModsModManualLinkAsync(modId, userId, username, ct);
    }

    public async Task<StringApiResultModel> RemoveNexusModsModManualLinkAsync(int modId, int? userId = null, string? username = null, CancellationToken ct = default)
    {
        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new StringApiResultModel("demo", null!);

        return await _implementation.RemoveNexusModsModManualLinkAsync(modId, userId, username, ct);
    }
    
    public async Task<StringApiResultModel> AddSteamWorkshopModManualLinkAsync(string modId, int? userId = null, string? username = null, CancellationToken ct = default)
    {
        return await _implementation.AddSteamWorkshopModManualLinkAsync(modId, userId, username, ct);
    }

    public async Task<StringApiResultModel> RemoveSteamWorkshopModManualLinkAsync(string modId, int? userId = null, string? username = null, CancellationToken ct = default)
    {
        return await _implementation.RemoveSteamWorkshopModManualLinkAsync(modId, userId, username, ct);
    }

    public async Task<UserManuallyLinkedSteamWorkshopModModelPagingDataApiResultModel> GetSteamWorkshopModManualLinkPaginatedAsync(PaginatedQuery? body = null, CancellationToken ct = default)
    {
        return await _implementation.GetSteamWorkshopModManualLinkPaginatedAsync(body, ct);
    }

    public async Task<UserManuallyLinkedNexusModsModModelPagingDataApiResultModel> GetNexusModsModManualLinkPaginatedAsync(PaginatedQuery? body = null, CancellationToken ct = default)
    {
        if (body is null)
            return await _implementation.GetNexusModsModManualLinkPaginatedAsync(body, ct);

        var token = await _tokenContainer.GetTokenAsync(ct);
        if (token?.Type.Equals("demo", StringComparison.OrdinalIgnoreCase) == true)
            return new UserManuallyLinkedNexusModsModModelPagingDataApiResultModel(new UserManuallyLinkedNexusModsModModelPagingData(PagingAdditionalMetadata.Empty, new List<UserManuallyLinkedNexusModsModModel>(), new PagingMetadata(1, 1, body.PageSize, 1)), null!);

        return await _implementation.GetNexusModsModManualLinkPaginatedAsync(body, ct);
    }

    public async Task<UserAvailableNexusModsModModelPagingDataApiResultModel> GetNexusModsModsPaginateAvailabledAsync(PaginatedQuery body, CancellationToken ct = default)
    {
        return await _implementation.GetNexusModsModsPaginateAvailabledAsync(body, ct);
    }
}