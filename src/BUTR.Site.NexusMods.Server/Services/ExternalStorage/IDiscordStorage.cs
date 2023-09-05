using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models;

using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services;

public sealed record DiscordOAuthTokens(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);

public interface IDiscordStorage
{
    Task<DiscordOAuthTokens?> GetAsync(string userId);
    Task<bool> UpsertAsync(NexusModsUserId nexusModsUserId, string discordUserId, DiscordOAuthTokens tokens);
    Task<bool> RemoveAsync(NexusModsUserId nexusModsUserId, string discordUserId);
}

public sealed class DatabaseDiscordStorage : IDiscordStorage
{
    private readonly IAppDbContextRead _dbContextRead;
    private readonly IAppDbContextWrite _dbContextWrite;

    public DatabaseDiscordStorage(IAppDbContextRead dbContextRead, IAppDbContextWrite dbContextWrite)
    {
        _dbContextRead = dbContextRead;
        _dbContextWrite = dbContextWrite;
    }

    public async Task<DiscordOAuthTokens?> GetAsync(string discordUserId)
    {
        var entity = await _dbContextRead.IntegrationDiscordTokens.FirstOrDefaultAsync(x => x.DiscordUserId.Equals(discordUserId));
        if (entity is null) return null;
        return new(entity.AccessToken, entity.RefreshToken, entity.AccessTokenExpiresAt);
    }

    public async Task<bool> UpsertAsync(NexusModsUserId nexusModsUserId, string discordUserId, DiscordOAuthTokens tokens)
    {
        var entityFactory = _dbContextWrite.CreateEntityFactory();
        await using var _ = _dbContextWrite.CreateSaveScope();

        var nexusModsUserToIntegrationDiscord = entityFactory.GetOrCreateNexusModsUserDiscord(nexusModsUserId, discordUserId);
        var tokensDiscord = entityFactory.GetOrCreateIntegrationDiscordTokens(nexusModsUserId, discordUserId, tokens.AccessToken, tokens.RefreshToken, tokens.ExpiresAt);

        _dbContextWrite.FutureUpsert(x => x.NexusModsUserToDiscord, nexusModsUserToIntegrationDiscord);
        _dbContextWrite.FutureUpsert(x => x.IntegrationDiscordTokens, tokensDiscord);
        return true;
    }

    public async Task<bool> RemoveAsync(NexusModsUserId nexusModsUserId, string discordUserId)
    {
        await _dbContextWrite.NexusModsUserToDiscord.Where(x => x.NexusModsUser.NexusModsUserId == nexusModsUserId && x.DiscordUserId == discordUserId).ExecuteDeleteAsync();
        await _dbContextWrite.IntegrationDiscordTokens.Where(x => x.DiscordUserId == discordUserId).ExecuteDeleteAsync();

        return true;
    }
}