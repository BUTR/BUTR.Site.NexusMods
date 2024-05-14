using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models;

using Microsoft.Extensions.DependencyInjection;

namespace BUTR.Site.NexusMods.Server.Extensions;

public static class IServiceScopeExtensions
{
    public static TServiceScope WithTenant<TServiceScope>(this TServiceScope serviceScope, TenantId tenant) where TServiceScope : IServiceScope
    {
        var tenantContextAccessor = serviceScope.ServiceProvider.GetRequiredService<ITenantContextAccessor>();
        tenantContextAccessor.Current = tenant;
        return serviceScope;
    }
}