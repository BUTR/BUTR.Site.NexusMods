using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;

using Microsoft.AspNetCore.Http;

using System.Threading;

namespace BUTR.Site.NexusMods.Server.Contexts;

/// <summary>
/// Provides an implementation of <see cref="ITenantContextAccessor" /> based on the current execution context.
/// </summary>
public class TenantContextAccessor : ITenantContextAccessor
{
    private static readonly AsyncLocal<TenantContextHolder> _tenantContextCurrent = new();

    public TenantId Current
    {
        get
        {
            if (_tenantContextCurrent.Value is { Tenant: { } localTenant })
                return localTenant;

            if (_httpContextAccessor?.HttpContext?.GetTenant() is { } httpContextTenant)
                return httpContextTenant;

            return TenantId.None;
        }
        set
        {
            if (_tenantContextCurrent.Value is { } holder)
            {
                // Clear current TenantId trapped in the AsyncLocals, as its done.
                holder.Tenant = null;
            }

            // Use an object indirection to hold the TenantId in the AsyncLocal,
            // so it can be cleared in all ExecutionContexts when its cleared.
            _tenantContextCurrent.Value = new TenantContextHolder { Tenant = value };
        }
    }

    private readonly IHttpContextAccessor? _httpContextAccessor;

    public TenantContextAccessor(IHttpContextAccessor? httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private sealed class TenantContextHolder
    {
        public TenantId? Tenant;
    }
}