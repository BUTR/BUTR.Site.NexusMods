using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services;

public class TenantDelegatingHandler : DelegatingHandler
{
    private readonly TenantProvider _tenantProvider;

    public TenantDelegatingHandler(TenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var tenant = await _tenantProvider.GetTenantAsync();

        request.Headers.Add("Tenant", tenant.ToString());
        return await base.SendAsync(request, ct);
    }
}