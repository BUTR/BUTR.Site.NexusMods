using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Contexts;

public sealed class EntityFactory
{
    private readonly ITenantContextAccessor _tenantContextAccessor;
    private readonly IAppDbContextWrite _dbContextWrite;

    private readonly ConcurrentDictionary<NexusModsUserId, NexusModsUserEntity> _nexusModsUsers = new();
    private readonly ConcurrentDictionary<NexusModsUserId, NexusModsUserToNameEntity> _nexusModsUserNames = new();
    private readonly ConcurrentDictionary<NexusModsModId, NexusModsModEntity> _nexusModsMods = new();
    private readonly ConcurrentDictionary<ModuleId, ModuleEntity> _modules = new();
    private readonly ConcurrentDictionary<ExceptionTypeId, ExceptionTypeEntity> _exceptionTypes = new();
    private readonly ConcurrentDictionary<string, NexusModsUserToIntegrationGitHubEntity> _gitHubUsers = new();
    private readonly ConcurrentDictionary<string, NexusModsUserToIntegrationDiscordEntity> _discordUsers = new();
    private readonly ConcurrentDictionary<string, NexusModsUserToIntegrationSteamEntity> _steamUsers = new();
    private readonly ConcurrentDictionary<string, NexusModsUserToIntegrationGOGEntity> _gogUsers = new();
    private readonly ConcurrentDictionary<string, IntegrationGitHubTokensEntity> _gitHubTokens = new();
    private readonly ConcurrentDictionary<string, IntegrationDiscordTokensEntity> _discordTokens = new();
    private readonly ConcurrentDictionary<string, IntegrationGOGTokensEntity> _gogTokens = new();
    private readonly ConcurrentDictionary<string, IntegrationSteamTokensEntity> _steamTokens = new();

    private readonly SemaphoreSlim _lock = new(1);

    public EntityFactory(ITenantContextAccessor tenantContextAccessor, IAppDbContextWrite dbContextWrite)
    {
        _tenantContextAccessor = tenantContextAccessor;
        _dbContextWrite = dbContextWrite;
    }

    public NexusModsUserEntity GetOrCreateNexusModsUser(NexusModsUserId nexusModsUserId)
    {
        return _nexusModsUsers.GetOrAdd(nexusModsUserId, ValueFactory);

        static NexusModsUserEntity ValueFactory(NexusModsUserId id) => NexusModsUserEntity.Create(id);
    }

    public NexusModsUserEntity GetOrCreateNexusModsUserWithName(NexusModsUserId nexusModsUserId, NexusModsUserName nexusModsUserName)
    {
        var user = _nexusModsUsers.GetOrAdd(nexusModsUserId, ValueFactory, nexusModsUserName);
        _ = _nexusModsUserNames.GetOrAdd(nexusModsUserId, ValueFactory2, user);
        return user;

        static NexusModsUserEntity ValueFactory(NexusModsUserId id, NexusModsUserName name) => NexusModsUserEntity.CreateWithName(id, name);
        static NexusModsUserToNameEntity ValueFactory2(NexusModsUserId id, NexusModsUserEntity user) => user.Name!;
    }

    public NexusModsUserToIntegrationSteamEntity GetOrCreateNexusModsUserSteam(NexusModsUserId nexusModsUserId, string steamUserId)
    {
        return _steamUsers.GetOrAdd(steamUserId, ValueFactory, (this, nexusModsUserId));

        static NexusModsUserToIntegrationSteamEntity ValueFactory(string steamUserId_, (EntityFactory, NexusModsUserId) tuple) => new()
        {
            NexusModsUser = tuple.Item1.GetOrCreateNexusModsUser(tuple.Item2),
            SteamUserId = steamUserId_,
        };
    }

    public NexusModsUserToIntegrationGitHubEntity GetOrCreateNexusModsUserGitHub(NexusModsUserId nexusModsUserId, string gitHubUserId)
    {
        return _gitHubUsers.GetOrAdd(gitHubUserId, ValueFactory, (this, nexusModsUserId));

        static NexusModsUserToIntegrationGitHubEntity ValueFactory(string gitHubUserId_, (EntityFactory, NexusModsUserId) tuple) => new()
        {
            NexusModsUser = tuple.Item1.GetOrCreateNexusModsUser(tuple.Item2),
            GitHubUserId = gitHubUserId_,
        };
    }

    public NexusModsUserToIntegrationDiscordEntity GetOrCreateNexusModsUserDiscord(NexusModsUserId nexusModsUserId, string discordUserId)
    {
        return _discordUsers.GetOrAdd(discordUserId, ValueFactory, (this, nexusModsUserId));

        static NexusModsUserToIntegrationDiscordEntity ValueFactory(string discordUserId_, (EntityFactory, NexusModsUserId) tuple) => new()
        {
            NexusModsUser = tuple.Item1.GetOrCreateNexusModsUser(tuple.Item2),
            DiscordUserId = discordUserId_,
        };
    }

    public NexusModsUserToIntegrationGOGEntity GetOrCreateNexusModsUserGOG(NexusModsUserId nexusModsUserId, string gogUserId)
    {
        return _gogUsers.GetOrAdd(gogUserId, ValueFactory, (this, nexusModsUserId));

        static NexusModsUserToIntegrationGOGEntity ValueFactory(string gogUserId_, (EntityFactory, NexusModsUserId) tuple) => new()
        {
            NexusModsUser = tuple.Item1.GetOrCreateNexusModsUser(tuple.Item2),
            GOGUserId = gogUserId_,
        };
    }

    public IntegrationGitHubTokensEntity GetOrCreateIntegrationGitHubTokens(NexusModsUserId nexusModsUserId, string gitHubUserId, string accessToken)
    {
        return _gitHubTokens.GetOrAdd(gitHubUserId, ValueFactory, (this, nexusModsUserId, accessToken));

        static IntegrationGitHubTokensEntity ValueFactory(string gitHubUserId, (EntityFactory, NexusModsUserId, string) tuple) => new()
        {
            GitHubUserId = gitHubUserId,
            NexusModsUser = tuple.Item1.GetOrCreateNexusModsUser(tuple.Item2),
            AccessToken = tuple.Item3,
            //UserToDiscord = GetOrCreateNexusModsUserDiscord(nexusModsUserId, discordUserId),
        };
    }

    public IntegrationDiscordTokensEntity GetOrCreateIntegrationDiscordTokens(NexusModsUserId nexusModsUserId, string discordUserId, string accessToken, string refreshToken, DateTimeOffset accessTokenExpiresAt)
    {
        return _discordTokens.GetOrAdd(discordUserId, ValueFactory, (this, nexusModsUserId, accessToken, refreshToken, accessTokenExpiresAt));

        static IntegrationDiscordTokensEntity ValueFactory(string discordUserId, (EntityFactory, NexusModsUserId, string, string, DateTimeOffset) tuple) => new()
        {
            DiscordUserId = discordUserId,
            NexusModsUser = tuple.Item1.GetOrCreateNexusModsUser(tuple.Item2),
            AccessToken = tuple.Item3,
            RefreshToken = tuple.Item4,
            AccessTokenExpiresAt = tuple.Item5,
            //UserToDiscord = GetOrCreateNexusModsUserDiscord(nexusModsUserId, discordUserId),
        };
    }

    public IntegrationGOGTokensEntity GetOrCreateIntegrationGOGTokens(NexusModsUserId nexusModsUserId, string gogUserId, string accessToken, string refreshToken, DateTimeOffset accessTokenExpiresAt)
    {
        return _gogTokens.GetOrAdd(gogUserId, ValueFactory, (this, nexusModsUserId, accessToken, refreshToken, accessTokenExpiresAt));

        static IntegrationGOGTokensEntity ValueFactory(string gogUserId, (EntityFactory, NexusModsUserId, string, string, DateTimeOffset) tuple) => new()
        {
            GOGUserId = gogUserId,
            NexusModsUser = tuple.Item1.GetOrCreateNexusModsUser(tuple.Item2),
            AccessToken = tuple.Item3,
            RefreshToken = tuple.Item4,
            AccessTokenExpiresAt = tuple.Item5,
            //UserToGOG = GetOrCreateNexusModsUserGOG(nexusModsUserId, gogUserId),
        };
    }

    public IntegrationSteamTokensEntity GetOrCreateIntegrationSteamTokens(NexusModsUserId nexusModsUserId, string steamUserId, Dictionary<string, string> data)
    {
        return _steamTokens.GetOrAdd(steamUserId, ValueFactory, (this, nexusModsUserId, data));

        static IntegrationSteamTokensEntity ValueFactory(string steamUserId, (EntityFactory, NexusModsUserId, Dictionary<string, string>) tuple) => new()
        {
            SteamUserId = steamUserId,
            NexusModsUser = tuple.Item1.GetOrCreateNexusModsUser(tuple.Item2),
            Data = tuple.Item3,
            //UserToSteam = GetOrCreateNexusModsUserSteam(nexusModsUserId, steamUserId),
        };
    }

    public NexusModsModEntity GetOrCreateNexusModsMod(NexusModsModId nexusModsModId)
    {
        var tenant = _tenantContextAccessor.Current;
        return _nexusModsMods.GetOrAdd(nexusModsModId, ValueFactory, tenant);

        static NexusModsModEntity ValueFactory(NexusModsModId id, TenantId tenant) => NexusModsModEntity.Create(tenant, id);
    }

    public ModuleEntity GetOrCreateModule(ModuleId moduleId)
    {
        var tenant = _tenantContextAccessor.Current;
        return _modules.GetOrAdd(moduleId, ValueFactory, tenant);

        static ModuleEntity ValueFactory(ModuleId id, TenantId tenant) => ModuleEntity.Create(tenant, id);
    }

    public ExceptionTypeEntity GetOrCreateExceptionType(ExceptionTypeId exception)
    {
        var tenant = _tenantContextAccessor.Current;
        return _exceptionTypes.GetOrAdd(ExceptionTypeEntity.Create(tenant, exception).ExceptionTypeId, ValueFactory, tenant);

        static ExceptionTypeEntity ValueFactory(ExceptionTypeId id, TenantId tenant) => ExceptionTypeEntity.Create(tenant, id);
    }

    public async Task<bool> SaveCreatedAsync(CancellationToken ct)
    {
        await _lock.WaitAsync(ct);

        try
        {
            var hasChange = false;
            async Task DoChange(Func<Task> action)
            {
                hasChange = true;
                await action();
            }

            if (!_nexusModsUsers.IsEmpty) await DoChange(() => _dbContextWrite.NexusModsUsers.UpsertAsync(_nexusModsUsers.Values));
            if (!_nexusModsUserNames.IsEmpty) await DoChange(() => _dbContextWrite.NexusModsUserToName.UpsertAsync(_nexusModsUserNames.Values));
            if (!_nexusModsMods.IsEmpty) await DoChange(() => _dbContextWrite.NexusModsMods.UpsertAsync(_nexusModsMods.Values));
            if (!_modules.IsEmpty) await DoChange(() => _dbContextWrite.Modules.UpsertAsync(_modules.Values));
            if (!_exceptionTypes.IsEmpty) await DoChange(() => _dbContextWrite.ExceptionTypes.UpsertAsync(_exceptionTypes.Values));
            if (!_gitHubUsers.IsEmpty) await DoChange(() => _dbContextWrite.NexusModsUserToGitHub.UpsertAsync(_gitHubUsers.Values));
            if (!_discordUsers.IsEmpty) await DoChange(() => _dbContextWrite.NexusModsUserToDiscord.UpsertAsync(_discordUsers.Values));
            if (!_steamUsers.IsEmpty) await DoChange(() => _dbContextWrite.NexusModsUserToSteam.UpsertAsync(_steamUsers.Values));
            if (!_gogUsers.IsEmpty) await DoChange(() => _dbContextWrite.NexusModsUserToGOG.UpsertAsync(_gogUsers.Values));
            if (!_gitHubTokens.IsEmpty) await DoChange(() => _dbContextWrite.IntegrationGitHubTokens.UpsertAsync(_gitHubTokens.Values));
            if (!_discordTokens.IsEmpty) await DoChange(() => _dbContextWrite.IntegrationDiscordTokens.UpsertAsync(_discordTokens.Values));
            if (!_gogTokens.IsEmpty) await DoChange(() => _dbContextWrite.IntegrationGOGTokens.UpsertAsync(_gogTokens.Values));
            if (!_steamTokens.IsEmpty) await DoChange(() => _dbContextWrite.IntegrationSteamTokens.UpsertAsync(_steamTokens.Values));

            return hasChange;
        }
        finally
        {
            _lock.Release();
        }
    }
}