using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Repositories;

using EFCore.BulkExtensions;

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Contexts;

/// <summary>
/// Upsert is a pain in the ass, especially the graph inclusion.
/// Instead, we manually track such entities and save them manually
/// </summary>
public sealed class UpsertEntityFactory : IUpsertEntityFactory
{
    private readonly ITenantContextAccessor _tenantContextAccessor;
    private readonly AppDbContextWrite _dbContextWrite;

    private readonly ConcurrentDictionary<NexusModsUserId, NexusModsUserEntity> _nexusModsUsers = new();
    private readonly ConcurrentDictionary<NexusModsUserId, NexusModsUserToNameEntity> _nexusModsUserNames = new();
    private readonly ConcurrentDictionary<NexusModsModId, NexusModsModEntity> _nexusModsMods = new();
    private readonly ConcurrentDictionary<ModuleId, ModuleEntity> _modules = new();
    private readonly ConcurrentDictionary<ExceptionTypeId, ExceptionTypeEntity> _exceptionTypes = new();

    private readonly SemaphoreSlim _lock = new(1);

    public UpsertEntityFactory(ITenantContextAccessor tenantContextAccessor, AppDbContextWrite dbContextWrite)
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
            async Task DoChangeAsync(Func<Task> action)
            {
                hasChange = true;
                await action();
            }

            if (!_nexusModsUsers.IsEmpty) await DoChangeAsync(() => _dbContextWrite.BulkInsertOrUpdateAsync(_nexusModsUsers.Values, o => o.IncludeGraph = false, cancellationToken: ct));
            if (!_nexusModsUserNames.IsEmpty) await DoChangeAsync(() => _dbContextWrite.BulkInsertOrUpdateAsync(_nexusModsUserNames.Values, o => o.IncludeGraph = false, cancellationToken: ct));
            if (!_nexusModsMods.IsEmpty) await DoChangeAsync(() => _dbContextWrite.BulkInsertOrUpdateAsync(_nexusModsMods.Values, o => o.IncludeGraph = false, cancellationToken: ct));
            if (!_modules.IsEmpty) await DoChangeAsync(() => _dbContextWrite.BulkInsertOrUpdateAsync(_modules.Values, o => o.IncludeGraph = false, cancellationToken: ct));
            if (!_exceptionTypes.IsEmpty) await DoChangeAsync(() => _dbContextWrite.BulkInsertOrUpdateAsync(_exceptionTypes.Values, o => o.IncludeGraph = false, cancellationToken: ct));

            return hasChange;
        }
        finally
        {
            _lock.Release();
        }
    }
}