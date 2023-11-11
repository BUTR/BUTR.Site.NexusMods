using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace BUTR.Site.NexusMods.Server.Contexts;

public sealed class TenantModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime)
    {
        if (designTime) return 0;

        var tenantContextAccessor = context.GetService<ITenantContextAccessor>();
        return tenantContextAccessor.Current;
    }
}