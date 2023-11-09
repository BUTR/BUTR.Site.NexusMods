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

    public async Task<int> GetTenantAsync()
    {
        if (! await _localStorage.ContainKeyAsync("tenant"))
            await SetTenantAsync(1);

        return await _localStorage.GetItemAsync<int>("tenant", CancellationToken.None);
    }

    public async Task SetTenantAsync(int tenant) => await _localStorage.SetItemAsync("tenant", tenant, CancellationToken.None);
}