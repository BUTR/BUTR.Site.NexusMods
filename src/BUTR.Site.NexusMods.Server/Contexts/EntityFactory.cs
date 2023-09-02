using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Shared;

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

    private readonly ConcurrentDictionary<int, NexusModsUserEntity> _nexusModsUsers = new();
    private readonly ConcurrentDictionary<ushort, NexusModsModEntity> _nexusModsMods = new();
    private readonly ConcurrentDictionary<string, ModuleEntity> _modules = new();
    private readonly ConcurrentDictionary<string, ExceptionTypeEntity> _exceptionTypes = new();
    private readonly ConcurrentDictionary<string, NexusModsUserToIntegrationDiscordEntity> _discordUsers = new();
    private readonly ConcurrentDictionary<string, NexusModsUserToIntegrationSteamEntity> _steamUsers = new();
    private readonly ConcurrentDictionary<string, NexusModsUserToIntegrationGOGEntity> _gogUsers = new();
    private readonly ConcurrentDictionary<string, IntegrationDiscordTokensEntity> _discordTokens = new();
    private readonly ConcurrentDictionary<string, IntegrationGOGTokensEntity> _gogTokens = new();
    private readonly ConcurrentDictionary<string, IntegrationSteamTokensEntity> _steamTokens = new();

    private const nint Saved = 1;
    private const nint Unsaved = 0;
    private nint _saved;

    public EntityFactory(ITenantContextAccessor tenantContextAccessor, IAppDbContextWrite dbContextWrite)
    {
        _tenantContextAccessor = tenantContextAccessor;
        _dbContextWrite = dbContextWrite;
    }

    public NexusModsUserEntity GetOrCreateNexusModsUser(int nexusModsUserId)
    {
        return _nexusModsUsers.GetOrAdd(nexusModsUserId, ValueFactory);

        static NexusModsUserEntity ValueFactory(int id) => NexusModsUserEntity.Create(id);
    }

    public NexusModsUserToIntegrationSteamEntity GetOrCreateNexusModsUserSteam(int nexusModsUserId, string steamUserId)
    {
        return _steamUsers.GetOrAdd(steamUserId, ValueFactory, nexusModsUserId);

        NexusModsUserToIntegrationSteamEntity ValueFactory(string steamUserId_, int nexusModsUserId_) => new()
        {
            NexusModsUser = GetOrCreateNexusModsUser(nexusModsUserId_),
            SteamUserId = steamUserId_,
        };
    }

    public NexusModsUserToIntegrationDiscordEntity GetOrCreateNexusModsUserDiscord(int nexusModsUserId, string discordUserId)
    {
        return _discordUsers.GetOrAdd(discordUserId, ValueFactory, nexusModsUserId);

        NexusModsUserToIntegrationDiscordEntity ValueFactory(string discordUserId_, int nexusModsUserId_) => new()
        {
            NexusModsUser = GetOrCreateNexusModsUser(nexusModsUserId_),
            DiscordUserId = discordUserId_,
        };
    }

    public NexusModsUserToIntegrationGOGEntity GetOrCreateNexusModsUserGOG(int nexusModsUserId, string gogUserId)
    {
        return _gogUsers.GetOrAdd(gogUserId, ValueFactory, nexusModsUserId);

        NexusModsUserToIntegrationGOGEntity ValueFactory(string gogUserId_, int nexusModsUserId_) => new()
        {
            NexusModsUser = GetOrCreateNexusModsUser(nexusModsUserId_),
            GOGUserId = gogUserId_,
        };
    }

    public IntegrationDiscordTokensEntity GetOrCreateIntegrationDiscordTokens(int nexusModsUserId, string discordUserId, string accessToken, string refreshToken, DateTimeOffset accessTokenExpiresAt)
    {
        return _discordTokens.GetOrAdd(discordUserId, ValueFactory);

        IntegrationDiscordTokensEntity ValueFactory(string _) => new()
        {
            DiscordUserId = discordUserId,
            NexusModsUser = GetOrCreateNexusModsUser(nexusModsUserId),
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = accessTokenExpiresAt,
            //UserToDiscord = GetOrCreateNexusModsUserDiscord(nexusModsUserId, discordUserId),
        };
    }

    public IntegrationGOGTokensEntity GetOrCreateIntegrationGOGTokens(int nexusModsUserId, string gogUserId, string accessToken, string refreshToken, DateTimeOffset accessTokenExpiresAt)
    {
        return _gogTokens.GetOrAdd(gogUserId, ValueFactory);

        IntegrationGOGTokensEntity ValueFactory(string _) => new()
        {
            GOGUserId = gogUserId,
            NexusModsUser = GetOrCreateNexusModsUser(nexusModsUserId),
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = accessTokenExpiresAt,
            //UserToGOG = GetOrCreateNexusModsUserGOG(nexusModsUserId, gogUserId),
        };
    }

    public IntegrationSteamTokensEntity GetOrCreateIntegrationSteamTokens(int nexusModsUserId, string steamUserId, Dictionary<string, string> data)
    {
        return _steamTokens.GetOrAdd(steamUserId, ValueFactory);

        IntegrationSteamTokensEntity ValueFactory(string _) => new()
        {
            SteamUserId = steamUserId,
            NexusModsUser = GetOrCreateNexusModsUser(nexusModsUserId),
            Data = data,
            //UserToSteam = GetOrCreateNexusModsUserSteam(nexusModsUserId, steamUserId),
        };
    }

    public NexusModsUserEntity GetOrCreateNexusModsUserWithName(int nexusModsUserId, string nexusModsUserName)
    {
        return _nexusModsUsers.GetOrAdd(nexusModsUserId, ValueFactory, nexusModsUserName);

        static NexusModsUserEntity ValueFactory(int id, string name) => NexusModsUserEntity.CreateWithName(id, name);
    }

    public NexusModsModEntity GetOrCreateNexusModsMod(ushort nexusModsModId)
    {
        var tenant = _tenantContextAccessor.Current;
        if (tenant is null) throw new Exception();
        return _nexusModsMods.GetOrAdd(nexusModsModId, ValueFactory, tenant.Value);

        static NexusModsModEntity ValueFactory(ushort id, Tenant tenant) => NexusModsModEntity.Create(tenant, id);
    }

    public ModuleEntity GetOrCreateModule(string moduleId)
    {
        var tenant = _tenantContextAccessor.Current;
        if (tenant is null) throw new Exception();
        return _modules.GetOrAdd(moduleId, ValueFactory, tenant.Value);

        static ModuleEntity ValueFactory(string id, Tenant tenant) => ModuleEntity.Create(tenant, id);
    }

    public ExceptionTypeEntity GetOrCreateExceptionType(string exception)
    {
        var tenant = _tenantContextAccessor.Current;
        if (tenant is null) throw new Exception();
        return _exceptionTypes.GetOrAdd(ExceptionTypeEntity.Create(tenant.Value, exception).ExceptionTypeId, ValueFactory, tenant.Value);

        static ExceptionTypeEntity ValueFactory(string id, Tenant tenant) => ExceptionTypeEntity.Create(tenant, id);
    }
    public ExceptionTypeEntity GetOrCreateExceptionTypeFromException(string exception)
    {
        var tenant = _tenantContextAccessor.Current;
        if (tenant is null) throw new Exception();
        return _exceptionTypes.GetOrAdd(ExceptionTypeEntity.FromException(tenant.Value, exception).ExceptionTypeId, ValueFactory, tenant.Value);

        static ExceptionTypeEntity ValueFactory(string id, Tenant tenant) => ExceptionTypeEntity.Create(tenant, id);
    }

    public async Task SaveCreated(CancellationToken ct)
    {
        var state = Interlocked.CompareExchange(ref _saved, Saved, Unsaved);
        if (state == Saved) return;

        try
        {
            if (!_nexusModsUsers.IsEmpty) await _dbContextWrite.NexusModsUsers.BulkMergeAsync(_nexusModsUsers.Values, ct);
            if (!_nexusModsMods.IsEmpty) await _dbContextWrite.NexusModsMods.BulkMergeAsync(_nexusModsMods.Values, ct);
            if (!_modules.IsEmpty) await _dbContextWrite.Modules.BulkMergeAsync(_modules.Values, ct);
            if (!_exceptionTypes.IsEmpty) await _dbContextWrite.ExceptionTypes.BulkMergeAsync(_exceptionTypes.Values, ct);
            if (!_discordUsers.IsEmpty) await _dbContextWrite.NexusModsUserToDiscord.BulkMergeAsync(_discordUsers.Values, ct);
            if (!_steamUsers.IsEmpty) await _dbContextWrite.NexusModsUserToSteam.BulkMergeAsync(_steamUsers.Values, ct);
            if (!_gogUsers.IsEmpty) await _dbContextWrite.NexusModsUserToGOG.BulkMergeAsync(_gogUsers.Values, ct);
            if (!_discordTokens.IsEmpty) await _dbContextWrite.IntegrationDiscordTokens.BulkMergeAsync(_discordTokens.Values, ct);
            if (!_gogTokens.IsEmpty) await _dbContextWrite.IntegrationGOGTokens.BulkMergeAsync(_gogTokens.Values, ct);
            if (!_steamTokens.IsEmpty) await _dbContextWrite.IntegrationSteamTokens.BulkMergeAsync(_steamTokens.Values, ct);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}