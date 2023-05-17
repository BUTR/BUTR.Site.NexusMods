using BUTR.Site.NexusMods.Server.Models.Database;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Models.API;

/// <summary>
/// This is an optimized class for json serialization of the Pagination functionality.
/// We start with the <see cref="BUTR.Site.NexusMods.Server.Models.Database.Paging{T}"/> class.
/// With the EF Core query provided, it will capture the current time and start database read with <see cref="System.Linq.Queryable.Count"/>.
/// The Read of the data will not be yet done. It will start lazily after the json serializer will start to serialize it.
/// After the serialization is done, our Async wrappers will capture the end time to be consumed by json next.
/// It's quite complex, but we avoid data bufferization on the server and also start to send the data to the client as early as possible.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed record PagingData<T> where T : class
{
    public static PagingData<T> Create<TOriginal>(Paging<TOriginal> data, Func<IAsyncEnumerable<TOriginal>, IAsyncEnumerable<T>> transformation) where TOriginal : class => new()
    {
        StartTime = data.StartTime,
        Metadata = data.Metadata,
        Items = transformation(data.Items),
    };
    public static PagingData<T> Create(Paging<T> data, Func<IAsyncEnumerable<T>, IAsyncEnumerable<T>> transformation) => new()
    {
        StartTime = data.StartTime,
        Metadata = data.Metadata,
        Items = transformation(data.Items),
    };
    public static PagingData<T> Create(Paging<T> data) => new()
    {
        StartTime = data.StartTime,
        Metadata = data.Metadata,
        Items = data.Items,
    };

    // Keep as classes because of state machine boxing
    private class AsyncEnumerableWrapper : IAsyncEnumerable<T>
    {
        private class AsyncEnumeratorWrapper : IAsyncEnumerator<T>
        {
            public T Current => _inner.Current;

            private readonly IAsyncEnumerator<T> _inner;
            private readonly Action _onLastItem;

            public AsyncEnumeratorWrapper(IAsyncEnumerator<T> inner, Action onLastItem) => (_inner, _onLastItem) = (inner, onLastItem);

            public async ValueTask<bool> MoveNextAsync()
            {
                var result = await _inner.MoveNextAsync();
                if (!result) _onLastItem.Invoke();
                return result;
            }

            public ValueTask DisposeAsync() => _inner.DisposeAsync();
        }

        private readonly IAsyncEnumerable<T> _inner;
        private readonly Action _onLastItem;

        public AsyncEnumerableWrapper(IAsyncEnumerable<T> inner, Action onLastItem) => (_inner, _onLastItem) = (inner, onLastItem);

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) => new AsyncEnumeratorWrapper(_inner.GetAsyncEnumerator(cancellationToken), _onLastItem);
    }

    [JsonIgnore]
    private long StartTime { get; set; }
    [JsonIgnore]
    private long EndTime { get; set; }

    [JsonPropertyOrder(3)]
    public uint QueryExecutionTimeMilliseconds => (uint) Stopwatch.GetElapsedTime(StartTime, EndTime).TotalMilliseconds;

    private readonly IAsyncEnumerable<T> _items = default!;
    [JsonPropertyOrder(2)]
    public required IAsyncEnumerable<T> Items { get => _items; init => _items = new AsyncEnumerableWrapper(value, () => EndTime = Stopwatch.GetTimestamp()); }

    [JsonPropertyOrder(1)]
    public required PagingMetadata Metadata { get; init; }

    private PagingData() { }
}