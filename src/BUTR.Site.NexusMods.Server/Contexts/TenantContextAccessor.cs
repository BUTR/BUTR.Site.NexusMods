using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Shared;

using Microsoft.AspNetCore.Http;

using System.Threading;

namespace BUTR.Site.NexusMods.Server.Contexts;

/// <summary>
/// Provides an implementation of <see cref="ITenantContextAccessor" /> based on the current execution context.
/// </summary>
public class TenantContextAccessor : ITenantContextAccessor
{
    private static readonly AsyncLocal<TenantContextHolder> _tenantContextCurrent = new();

    public Tenant? Current
    {
        get
        {
            if (_tenantContextCurrent.Value is { Tenant: { } localTenant })
                return localTenant;

            if (_httpContextAccessor?.HttpContext?.GetTenant() is { } httpContextTenant)
                return httpContextTenant;

            return Tenant.Bannerlord;
        }
        set
        {
            if (_tenantContextCurrent.Value is { } holder)
            {
                // Clear current HttpContext trapped in the AsyncLocals, as its done.
                holder.Tenant = null;
            }

            if (value != null)
            {
                // Use an object indirection to hold the HttpContext in the AsyncLocal,
                // so it can be cleared in all ExecutionContexts when its cleared.
                _tenantContextCurrent.Value = new TenantContextHolder { Tenant = value };
            }
        }
    }

    private readonly IHttpContextAccessor? _httpContextAccessor;

    public TenantContextAccessor(IHttpContextAccessor? httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private sealed class TenantContextHolder
    {
        public Tenant? Tenant;
    }
}