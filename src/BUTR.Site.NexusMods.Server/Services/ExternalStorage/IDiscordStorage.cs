using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Repositories;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services;

public sealed record DiscordOAuthTokens(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);

public interface IDiscordStorage
{
    Task<DiscordOAuthTokens?> GetAsync(string userId);
    Task<bool> UpsertAsync(NexusModsUserId nexusModsUserId, string discordUserId, DiscordOAuthTokens tokens);
    Task<bool> RemoveAsync(NexusModsUserId nexusModsUserId, string discordUserId);
}

[ScopedService<IDiscordStorage>]
public sealed class DatabaseDiscordStorage : IDiscordStorage
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public DatabaseDiscordStorage(IUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task<DiscordOAuthTokens?> GetAsync(string discordUserId)
    {
        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead(TenantId.None);

        var entity = await unitOfRead.IntegrationDiscordTokens.FirstOrDefaultAsync(x => x.DiscordUserId.Equals(discordUserId), null, CancellationToken.None);
        if (entity is null) return null;
        return new(entity.AccessToken, entity.RefreshToken, entity.AccessTokenExpiresAt);
    }

    public async Task<bool> UpsertAsync(NexusModsUserId nexusModsUserId, string discordUserId, DiscordOAuthTokens tokens)
    {
        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite(TenantId.None);

        var nexusModsUserToIntegrationDiscord = new NexusModsUserToIntegrationDiscordEntity
        {
            NexusModsUserId = nexusModsUserId,
            NexusModsUser = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsUser(nexusModsUserId),
            DiscordUserId = discordUserId,
        };
        var tokensDiscord = new IntegrationDiscordTokensEntity
        {
            DiscordUserId = discordUserId,
            NexusModsUserId = nexusModsUserId,
            NexusModsUser = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsUser(nexusModsUserId),
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            AccessTokenExpiresAt = tokens.ExpiresAt,
        };

        unitOfWrite.NexusModsUserToDiscord.Upsert(nexusModsUserToIntegrationDiscord);
        unitOfWrite.IntegrationDiscordTokens.Upsert(tokensDiscord);

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        return true;
    }

    public async Task<bool> RemoveAsync(NexusModsUserId nexusModsUserId, string discordUserId)
    {
        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite(TenantId.None);

        unitOfWrite.NexusModsUserToDiscord.Remove(x => x.NexusModsUser.NexusModsUserId == nexusModsUserId && x.DiscordUserId == discordUserId);
        unitOfWrite.IntegrationDiscordTokens.Remove(x => x.DiscordUserId == discordUserId);

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        return true;
    }
}