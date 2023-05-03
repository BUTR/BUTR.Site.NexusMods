using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services;

public sealed record Token(string Type, string Value);

public interface ITokenContainer
{
    event Action OnTokenChanged;

    Task<Token?> GetTokenAsync(CancellationToken ct = default);
    Task SetTokenAsync(Token? token, CancellationToken ct = default);
    Task RefreshTokenAsync(Token token, CancellationToken ct = default);
}