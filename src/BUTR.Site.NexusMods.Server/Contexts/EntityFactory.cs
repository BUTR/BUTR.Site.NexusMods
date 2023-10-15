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
    private readonly ConcurrentDictionary<string, NexusModsUserToIntegrationDiscordEntity> _discordUsers = new();
    private readonly ConcurrentDictionary<string, NexusModsUserToIntegrationSteamEntity> _steamUsers = new();
    private readonly ConcurrentDictionary<string, NexusModsUserToIntegrationGOGEntity> _gogUsers = new();
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
    public ExceptionTypeEntity GetOrCreateExceptionTypeFromException(string exception)
    {
        var tenant = _tenantContextAccessor.Current;
        return _exceptionTypes.GetOrAdd(ExceptionTypeEntity.FromException(tenant, exception).ExceptionTypeId, ValueFactory, tenant);

        static ExceptionTypeEntity ValueFactory(ExceptionTypeId id, TenantId tenant) => ExceptionTypeEntity.Create(tenant, id);
    }

    public async Task SaveCreatedAsync(CancellationToken ct)
    {
        await _lock.WaitAsync(ct);

        try
        {
            if (!_nexusModsUsers.IsEmpty) await _dbContextWrite.NexusModsUsers.BulkMergeAsync(_nexusModsUsers.Values, o => o.UseInternalTransaction = false, ct);
            if (!_nexusModsUserNames.IsEmpty) await _dbContextWrite.NexusModsUserToName.BulkMergeAsync(_nexusModsUserNames.Values, o => o.UseInternalTransaction = false, ct);
            if (!_nexusModsMods.IsEmpty) await _dbContextWrite.NexusModsMods.BulkMergeAsync(_nexusModsMods.Values, o => o.UseInternalTransaction = false, ct);
            if (!_modules.IsEmpty) await _dbContextWrite.Modules.BulkMergeAsync(_modules.Values, o => o.UseInternalTransaction = false, ct);
            if (!_exceptionTypes.IsEmpty) await _dbContextWrite.ExceptionTypes.BulkMergeAsync(_exceptionTypes.Values, o => o.UseInternalTransaction = false, ct);
            if (!_discordUsers.IsEmpty) await _dbContextWrite.NexusModsUserToDiscord.BulkMergeAsync(_discordUsers.Values, o => o.UseInternalTransaction = false, ct);
            if (!_steamUsers.IsEmpty) await _dbContextWrite.NexusModsUserToSteam.BulkMergeAsync(_steamUsers.Values, o => o.UseInternalTransaction = false, ct);
            if (!_gogUsers.IsEmpty) await _dbContextWrite.NexusModsUserToGOG.BulkMergeAsync(_gogUsers.Values, o => o.UseInternalTransaction = false, ct);
            if (!_discordTokens.IsEmpty) await _dbContextWrite.IntegrationDiscordTokens.BulkMergeAsync(_discordTokens.Values, o => o.UseInternalTransaction = false, ct);
            if (!_gogTokens.IsEmpty) await _dbContextWrite.IntegrationGOGTokens.BulkMergeAsync(_gogTokens.Values, o => o.UseInternalTransaction = false, ct);
            if (!_steamTokens.IsEmpty) await _dbContextWrite.IntegrationSteamTokens.BulkMergeAsync(_steamTokens.Values, o => o.UseInternalTransaction = false, ct);
        }
        finally
        {
            _lock.Release();
        }
    }
}