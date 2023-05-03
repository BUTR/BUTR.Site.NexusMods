using Blazored.SessionStorage;

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services;

public sealed record CacheOptions
{
    public DateTimeOffset? AbsoluteExpiration { get; init; }
    public IChangeToken? ChangeToken { get; init; }
}

public sealed class StorageCache : IAsyncDisposable
{
    private record EntryOptions<T>
    {
        public required T Value { get; init; }
        public required DateTimeOffset? AbsoluteExpiration { get; init; }
    }

    private readonly ISessionStorageService _storage;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    private readonly List<IChangeToken> _expirationTokens = new();
    private readonly List<IDisposable> _expirationTokenRegistrations = new();
    private readonly List<CancellationTokenSource> _cancellationTokenSources = new();
    private readonly ConcurrentDictionary<string, object> _tasksInProgress = new();

    public StorageCache(ISessionStorageService storage, IOptions<JsonSerializerOptions> jsonSerializerOptions)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _jsonSerializerOptions = jsonSerializerOptions.Value ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
    }

    public async Task<T?> GetAsync<T>(string key_, Func<Task<(T Value, CacheOptions Options)?>> factory, CancellationToken ct) where T : class
    {
        var key = $"cache_{key_}";

        async Task<T?> TryGetValue()
        {
            try
            {
                if (_tasksInProgress.GetOrAdd(key, static (_, func) => func(), factory) is not Task<(T Value, CacheOptions Options)?> task) return null;
                if (await task is not { } result) return null;
                await SetAsync(key, result.Value, result.Options, ct);
                return result.Value;
            }
            finally
            {
                _tasksInProgress.TryRemove(key, out _);
            }
        }

        if (await _storage.GetItemAsStringAsync(key, ct) is not { } json)
        {
            return await TryGetValue();
        }

        if (JsonSerializer.Deserialize<EntryOptions<T>?>(json, _jsonSerializerOptions) is not { } data)
        {
            return await TryGetValue();
        }

        if (DateTimeOffset.UtcNow > data.AbsoluteExpiration)
        {
            await RemoveAsync(key, ct);
            return await TryGetValue();
        }

        return data.Value;
    }

    public async Task SetAsync<T>(string key_, T value, CacheOptions options, CancellationToken ct)
    {
        var key = $"cache_{key_}";

        if (options.ChangeToken is { ActiveChangeCallbacks: true } changeToken)
        {
            var registration = changeToken.RegisterChangeCallback(tokenObj =>
            {
                if (tokenObj is CancellationToken token)
                    _ = RemoveAsync(key, token);
            }, ct);
            _expirationTokenRegistrations.Add(registration);
            _expirationTokens.Add(changeToken);
        }

        if (options.AbsoluteExpiration is not null && options.AbsoluteExpiration - DateTimeOffset.UtcNow is { } offset && offset > TimeSpan.Zero)
        {
            var cts = new CancellationTokenSource(offset);
            var cancellationChangeToken = new CancellationChangeToken(cts.Token);
            cancellationChangeToken.RegisterChangeCallback(tokenObj =>
            {
                if (tokenObj is CancellationToken token)
                    _ = RemoveAsync(key, token);
            }, ct);
            _cancellationTokenSources.Add(cts);
            _expirationTokens.Add(cancellationChangeToken);
        }

        var data = new EntryOptions<T> { Value = value, AbsoluteExpiration = options.AbsoluteExpiration };
        await _storage.SetItemAsStringAsync(key, JsonSerializer.Serialize(data, _jsonSerializerOptions), ct);
    }

    public async Task RemoveAsync(string key_, CancellationToken ct)
    {
        var key = $"cache_{key_}";

        await _storage.RemoveItemAsync(key, ct);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var cts in _cancellationTokenSources)
        {
            if (cts is IAsyncDisposable disposable)
                await disposable.DisposeAsync();
            else
                cts.Dispose();
        }
        foreach (var tokenRegistration in _expirationTokenRegistrations)
        {
            if (tokenRegistration is IAsyncDisposable disposable)
                await disposable.DisposeAsync();
            else
                tokenRegistration.Dispose();
        }
    }
}