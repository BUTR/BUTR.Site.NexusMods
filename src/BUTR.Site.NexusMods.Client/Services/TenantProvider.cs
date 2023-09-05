using Blazored.LocalStorage;

using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services;

public sealed class TenantProvider
{
    private readonly ILocalStorageService _localStorage;

    public TenantProvider(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task<string> GetTenantAsync()
    {
        if (! await _localStorage.ContainKeyAsync("tenant"))
            await SetTenantAsync("1");

        return await _localStorage.GetItemAsStringAsync("tenant", CancellationToken.None);
    }

    public async Task SetTenantAsync(string tenant) => await _localStorage.SetItemAsStringAsync("tenant", tenant, CancellationToken.None);
}