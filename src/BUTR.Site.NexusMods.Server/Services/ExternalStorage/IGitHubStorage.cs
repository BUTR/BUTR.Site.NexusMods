using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;

using Microsoft.EntityFrameworkCore;

using System.Linq;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services;

public sealed record GitHubOAuthTokens(string AccessToken);

public interface IGitHubStorage
{
    Task<GitHubOAuthTokens?> GetAsync(string userId);
    Task<bool> UpsertAsync(NexusModsUserId nexusModsUserId, string gitHubUserId, GitHubOAuthTokens tokens);
    Task<bool> RemoveAsync(NexusModsUserId nexusModsUserId, string gitHubUserId);
}

[ScopedService<IGitHubStorage>]
public sealed class DatabaseGitHubStorage : IGitHubStorage
{
    private readonly IAppDbContextRead _dbContextRead;
    private readonly IAppDbContextWrite _dbContextWrite;

    public DatabaseGitHubStorage(IAppDbContextRead dbContextRead, IAppDbContextWrite dbContextWrite)
    {
        _dbContextRead = dbContextRead;
        _dbContextWrite = dbContextWrite;
    }

    public async Task<GitHubOAuthTokens?> GetAsync(string gitHubUserId)
    {
        var entity = await _dbContextRead.IntegrationGitHubTokens.FirstOrDefaultAsync(x => x.GitHubUserId.Equals(gitHubUserId));
        if (entity is null) return null;
        return new(entity.AccessToken);
    }

    public async Task<bool> UpsertAsync(NexusModsUserId nexusModsUserId, string gitHubUserId, GitHubOAuthTokens tokens)
    {
        var entityFactory = _dbContextWrite.GetEntityFactory();
        await using var _ = await _dbContextWrite.CreateSaveScopeAsync();

        var nexusModsUserToIntegrationGitHub = entityFactory.GetOrCreateNexusModsUserGitHub(nexusModsUserId, gitHubUserId);
        var tokensGitHub = entityFactory.GetOrCreateIntegrationGitHubTokens(nexusModsUserId, gitHubUserId, tokens.AccessToken);

        await _dbContextWrite.NexusModsUserToGitHub.UpsertOnSaveAsync(nexusModsUserToIntegrationGitHub);
        await _dbContextWrite.IntegrationGitHubTokens.UpsertOnSaveAsync(tokensGitHub);
        return true;
    }

    public async Task<bool> RemoveAsync(NexusModsUserId nexusModsUserId, string gitHubUserId)
    {
        await _dbContextWrite.NexusModsUserToGitHub.Where(x => x.NexusModsUser.NexusModsUserId == nexusModsUserId && x.GitHubUserId == gitHubUserId).ExecuteDeleteAsync();
        await _dbContextWrite.IntegrationGitHubTokens.Where(x => x.GitHubUserId == gitHubUserId).ExecuteDeleteAsync();
        return true;
    }
}