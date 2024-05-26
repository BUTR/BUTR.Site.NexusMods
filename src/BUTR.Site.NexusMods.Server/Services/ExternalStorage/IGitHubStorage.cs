using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Repositories;

using System.Threading;
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
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public DatabaseGitHubStorage(IUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task<GitHubOAuthTokens?> GetAsync(string gitHubUserId)
    {
        await using var unitOfRead = _unitOfWorkFactory.CreateUnitOfRead(TenantId.None);

        var entity = await unitOfRead.IntegrationGitHubTokens.FirstOrDefaultAsync(x => x.GitHubUserId.Equals(gitHubUserId), null, CancellationToken.None);
        if (entity is null) return null;
        return new(entity.AccessToken);
    }

    public async Task<bool> UpsertAsync(NexusModsUserId nexusModsUserId, string gitHubUserId, GitHubOAuthTokens tokens)
    {
        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite(TenantId.None);

        var nexusModsUserToIntegrationGitHub = new NexusModsUserToIntegrationGitHubEntity
        {
            NexusModsUserId = nexusModsUserId,
            NexusModsUser = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsUser(nexusModsUserId),
            GitHubUserId = gitHubUserId,
        };
        var tokensGitHub = new IntegrationGitHubTokensEntity
        {
            GitHubUserId = gitHubUserId,
            NexusModsUserId = nexusModsUserId,
            NexusModsUser = unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsUser(nexusModsUserId),
            AccessToken = tokens.AccessToken,
        };

        unitOfWrite.NexusModsUserToGitHub.Upsert(nexusModsUserToIntegrationGitHub);
        unitOfWrite.IntegrationGitHubTokens.Upsert(tokensGitHub);

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        return true;
    }

    public async Task<bool> RemoveAsync(NexusModsUserId nexusModsUserId, string gitHubUserId)
    {
        await using var unitOfWrite = _unitOfWorkFactory.CreateUnitOfWrite(TenantId.None);

        unitOfWrite.NexusModsUserToGitHub.Remove(x => x.NexusModsUser.NexusModsUserId == nexusModsUserId && x.GitHubUserId == gitHubUserId);
        unitOfWrite.IntegrationGitHubTokens.Remove(x => x.GitHubUserId == gitHubUserId);

        await unitOfWrite.SaveChangesAsync(CancellationToken.None);
        return true;
    }
}