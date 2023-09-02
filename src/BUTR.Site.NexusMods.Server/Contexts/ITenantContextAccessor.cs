using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Shared;

namespace BUTR.Site.NexusMods.Server.Contexts;

/// <summary>
/// Provides access to the current <see cref="Tenant"/>, if one is available.
/// </summary>
/// <remarks>
/// This interface should be used with caution. It relies on <see cref="System.Threading.AsyncLocal{T}" /> which can have a negative performance impact on async calls.
/// </remarks>
public interface ITenantContextAccessor
{
    Tenant? Current { get; set; }
}