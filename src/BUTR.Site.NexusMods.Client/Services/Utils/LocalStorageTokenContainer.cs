using Blazored.LocalStorage;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services
{
    public sealed class LocalStorageTokenContainer : ITokenContainer
    {
        public event Action? OnTokenChanged;

        private readonly ILocalStorageService _localStorage;

        public LocalStorageTokenContainer(ILocalStorageService localStorage)
        {
            _localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
        }

        public async Task<Token?> GetTokenAsync(CancellationToken ct = default)
        {
            try
            {
                return await _localStorage.GetItemAsync<Token>("token");
            }
            catch (Exception e)
            {
                await _localStorage.RemoveItemAsync("token");
                return null;
            }
        }

        public async Task SetTokenAsync(Token? token, CancellationToken ct = default)
        {
            if (token is null)
            {
                await _localStorage.RemoveItemAsync("token");
            }
            else
            {
                await _localStorage.SetItemAsync("token", token, ct);
            }
            OnTokenChanged?.Invoke();
        }
    }
}