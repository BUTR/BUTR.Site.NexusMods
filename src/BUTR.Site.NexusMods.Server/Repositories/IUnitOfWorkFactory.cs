using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models;

using Microsoft.Extensions.DependencyInjection;

using System;

namespace BUTR.Site.NexusMods.Server.Repositories;

[ScopedService<IUnitOfWorkFactory>]
internal class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly IServiceProvider _serviceProvider;

    public UnitOfWorkFactory(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public IUnitOfRead CreateUnitOfRead()
    {
        var tenantContextAccessor = _serviceProvider.GetRequiredService<ITenantContextAccessor>();
        if (tenantContextAccessor.Current == TenantId.Error)
            throw new InvalidOperationException("Tenant context is not set.");

        return _serviceProvider.GetRequiredService<IUnitOfRead>();
    }
    public IUnitOfRead CreateUnitOfRead(TenantId tenant)
    {
        var tenantContextAccessor = _serviceProvider.GetRequiredService<ITenantContextAccessor>();
        tenantContextAccessor.Current = tenant;

        return _serviceProvider.GetRequiredService<IUnitOfRead>();
    }

    public IUnitOfWrite CreateUnitOfWrite()
    {
        var tenantContextAccessor = _serviceProvider.GetRequiredService<ITenantContextAccessor>();
        if (tenantContextAccessor.Current == TenantId.Error)
            throw new InvalidOperationException("Tenant context is not set.");

        return _serviceProvider.GetRequiredService<IUnitOfWrite>();
    }

    public IUnitOfWrite CreateUnitOfWrite(TenantId tenant)
    {
        var tenantContextAccessor = _serviceProvider.GetRequiredService<ITenantContextAccessor>();
        tenantContextAccessor.Current = tenant;

        return _serviceProvider.GetRequiredService<IUnitOfWrite>();
    }
}