using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Repositories;

public sealed record UserAvailableModModel
{
    public required NexusModsModId NexusModsModId { get; init; }
    public required string Name { get; init; }
}

public sealed record UserLinkedModModel
{
    public required NexusModsModId NexusModsModId { get; init; }
    public required string Name { get; init; }
    public required NexusModsUserId[] OwnerNexusModsUserIds { get; init; }
    public required NexusModsUserId[] AllowedNexusModsUserIds { get; init; }
    public required NexusModsUserId[] ManuallyLinkedNexusModsUserIds { get; init; }
    public required ModuleId[] ManuallyLinkedModuleIds { get; init; }
    public required ModuleId[] KnownModuleIds { get; init; }
}

public interface INexusModsUserRepositoryRead : IRepositoryRead<NexusModsUserEntity>
{
    Task<NexusModsUserEntity?> GetUserWithIntegrationsAsync(NexusModsUserId userId, CancellationToken ct);
    Task<NexusModsUserEntity> GetUserAsync(NexusModsUserId userId, CancellationToken ct);

    Task<Paging<UserLinkedModModel>> GetNexusModsModsPaginatedAsync(NexusModsUserId userId, PaginatedQuery query, CancellationToken ct);

    Task<Paging<UserAvailableModModel>> GetAvailableModsPaginatedAsync(NexusModsUserId userId, PaginatedQuery query, CancellationToken ct);

    Task<int> GetLinkedModCountAsync(NexusModsUserId userId, CancellationToken ct);
}
public interface INexusModsUserRepositoryWrite : IRepositoryWrite<NexusModsUserEntity>, INexusModsUserRepositoryRead;

[ScopedService<INexusModsUserRepositoryWrite, INexusModsUserRepositoryRead>]
internal class NexusModsUserRepository : Repository<NexusModsUserEntity>, INexusModsUserRepositoryWrite
{
    protected override IQueryable<NexusModsUserEntity> InternalQuery => base.InternalQuery
        .Include(x => x.Name)
        .Include(x => x.ToRoles)
        .Include(x => x.ToModules)
        .Include(x => x.ToNexusModsMods)
        .Include(x => x.ToCrashReports)
        .Include(x => x.ToArticles)
        .Include(x => x.ToGitHub)
        .Include(x => x.ToDiscord)
        .Include(x => x.ToSteam)
        .Include(x => x.ToGOG);

    public NexusModsUserRepository(IAppDbContextProvider appDbContextProvider) : base(appDbContextProvider.Get()) { }

    public async Task<NexusModsUserEntity> GetUserAsync(NexusModsUserId userId, CancellationToken ct) => await _dbContext.NexusModsUsers
        .Include(x => x.ToModules).ThenInclude(x => x.Module)
        .Include(x => x.ToNexusModsMods).ThenInclude(x => x.NexusModsMod)
        .AsSplitQuery()
        .FirstOrDefaultAsync(x => x.NexusModsUserId == userId, ct) ?? NexusModsUserEntity.Create(userId);

    public async Task<NexusModsUserEntity?> GetUserWithIntegrationsAsync(NexusModsUserId userId, CancellationToken ct) => await _dbContext.NexusModsUsers
        .Include(x => x.ToRoles)
        .Include(x => x.ToGitHub!).ThenInclude(x => x.ToTokens)
        .Include(x => x.ToDiscord!).ThenInclude(x => x.ToTokens)
        .Include(x => x.ToGOG!).ThenInclude(x => x.ToTokens)
        .Include(x => x.ToGOG!).ThenInclude(x => x.ToOwnedTenants)
        .Include(x => x.ToSteam!).ThenInclude(x => x.ToTokens)
        .Include(x => x.ToSteam!).ThenInclude(x => x.ToOwnedTenants)
        .AsSplitQuery()
        .FirstOrDefaultAsync(x => x.NexusModsUserId == userId, ct);

    public async Task<Paging<UserLinkedModModel>> GetNexusModsModsPaginatedAsync(NexusModsUserId userId, PaginatedQuery query, CancellationToken ct)
    {
        var availableModsByNexusModsModLinkage = _dbContext.NexusModsUsers
            .Include(x => x.ToNexusModsMods).ThenInclude(x => x.NexusModsMod).ThenInclude(x => x.Name)
            .Include(x => x.ToNexusModsMods).ThenInclude(x => x.NexusModsMod).ThenInclude(x => x.ToNexusModsUsers).ThenInclude(x => x.NexusModsUser)
            .Include(x => x.ToNexusModsMods).ThenInclude(x => x.NexusModsMod).ThenInclude(x => x.ModuleIds).ThenInclude(x => x.Module)
            .Where(x => x.NexusModsUserId == userId)
            .SelectMany(x => x.ToNexusModsMods)
            .Select(x => x.NexusModsMod)
            .AsSplitQuery()
            .Select(x => new UserLinkedModModel
            {
                NexusModsModId = x.NexusModsModId,
                Name = x.Name!.Name,
                OwnerNexusModsUserIds = x.ToNexusModsUsers.Where(y => y.NexusModsUser.NexusModsUserId != userId && y.LinkType == NexusModsUserToNexusModsModLinkType.ByAPIConfirmation).Select(y => y.NexusModsUser.NexusModsUserId).ToArray(),
                AllowedNexusModsUserIds = x.ToNexusModsUsers.Where(y => y.NexusModsUser.NexusModsUserId != userId && y.LinkType == NexusModsUserToNexusModsModLinkType.ByOwner || y.LinkType == NexusModsUserToNexusModsModLinkType.ByStaff).Select(y => y.NexusModsUser.NexusModsUserId).ToArray(),
                ManuallyLinkedNexusModsUserIds = x.ToNexusModsUsers.Where(y => y.NexusModsUser.NexusModsUserId != userId && y.LinkType == NexusModsUserToNexusModsModLinkType.ByOwner).Select(y => y.NexusModsUser.NexusModsUserId).ToArray(),
                ManuallyLinkedModuleIds = x.ModuleIds.Where(y => y.LinkType == NexusModsModToModuleLinkType.ByStaff).Select(y => y.Module.ModuleId).ToArray(),
                KnownModuleIds = x.ModuleIds.Where(y => y.LinkType == NexusModsModToModuleLinkType.ByUnverifiedFileExposure).Select(y => y.Module.ModuleId).ToArray(),
            });

        return await availableModsByNexusModsModLinkage
            //.PaginatedGroupedAsync(query, 20, new() { Property = nameof(UserLinkedModModel.NexusModsModId), Type = SortingType.Ascending }, ct);
            .PaginatedAsync(query, 20, new() { Property = nameof(UserLinkedModModel.NexusModsModId), Type = SortingType.Ascending }, ct);
    }

    public async Task<Paging<UserAvailableModModel>> GetAvailableModsPaginatedAsync(NexusModsUserId userId, PaginatedQuery query, CancellationToken ct)
    {
        var userToModIds = _dbContext.NexusModsUserToNexusModsMods
            .Include(x => x.NexusModsMod).ThenInclude(x => x.Name)
            .Where(x => x.NexusModsUser.NexusModsUserId == userId)
            .Select(x => x.NexusModsMod);

        var userToModuleIdsToModIds = _dbContext.NexusModsUserToModules
            .Include(x => x.Module).ThenInclude(x => x.ToNexusModsMods).ThenInclude(x => x.NexusModsMod).ThenInclude(x => x.Name)
            .AsSplitQuery()
            .Where(x => x.NexusModsUser.NexusModsUserId == userId)
            .Select(x => x.Module)
            .SelectMany(x => x.ToNexusModsMods)
            .Select(x => x.NexusModsMod);

        return await userToModIds.Union(userToModuleIdsToModIds)
            .Select(x => new UserAvailableModModel
            {
                NexusModsModId = x.NexusModsModId,
                Name = x.Name!.Name,
            })
            .PaginatedAsync(query, 20, new() { Property = nameof(NexusModsModEntity.NexusModsModId), Type = SortingType.Ascending }, ct);
    }


    public async Task<int> GetLinkedModCountAsync(NexusModsUserId userId, CancellationToken ct) => await _dbContext.NexusModsUsers
        .Include(x => x.ToNexusModsMods)
        .AsSplitQuery()
        .Where(x => x.NexusModsUserId == userId)
        .SelectMany(x => x.ToNexusModsMods)
        .CountAsync(ct);
}