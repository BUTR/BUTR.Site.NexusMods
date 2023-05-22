using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.ServerClient.Utils;

internal class OneTimeEnumerable<T> : IAsyncEnumerable<T>
{
    private class OneTimeEnumerator : IAsyncEnumerator<T>
    {
        public T Current => _inner.Current;

        private readonly IAsyncEnumerator<T> _inner;
        private readonly Action _onLastItem;

        public OneTimeEnumerator(IAsyncEnumerator<T> inner, Action onLastItem) => (_inner, _onLastItem) = (inner, onLastItem);

        public async ValueTask<bool> MoveNextAsync()
        {
            var result = await _inner.MoveNextAsync();
            if (!result) _onLastItem.Invoke();
            return result;
        }

        public ValueTask DisposeAsync() => _inner.DisposeAsync();
    }

    public TaskCompletionSource Enumeration { get; } = new();
    private readonly IAsyncEnumerator<T> _enumerator;

    public OneTimeEnumerable(IAsyncEnumerable<T> inner)
    {
        _enumerator = new OneTimeEnumerator(inner.GetAsyncEnumerator(), () => Enumeration.SetResult());
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) => _enumerator;
}