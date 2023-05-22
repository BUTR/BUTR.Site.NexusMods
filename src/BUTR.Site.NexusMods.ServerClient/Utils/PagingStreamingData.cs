using BUTR.Site.NexusMods.ServerClient.LazyTasks;

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;

namespace BUTR.Site.NexusMods.ServerClient.Utils;

public sealed record PagingStreamingData<T> where T : class
{
    public static readonly PagingStreamingData<T> Empty = Create();
    
    private static PagingStreamingData<T> Create()
    {
        LazyTask<PagingMetadata> GetMetadata() => LazyTask<PagingMetadata>.FromResult(PagingMetadata.Empty);
        LazyTask<IAsyncEnumerable<T>> GetItems() => LazyTask<IAsyncEnumerable<T>>.FromResult(AsyncEnumerable.Empty<T>());
        LazyTask<PagingAdditionalMetadata> GetQueryExecutionTimeMilliseconds() => LazyTask<PagingAdditionalMetadata>.FromResult(PagingAdditionalMetadata.Empty);

        return new PagingStreamingData<T>
        {
            Metadata = GetMetadata(),
            Items = GetItems(),
            AdditionalMetadata = GetQueryExecutionTimeMilliseconds(),
        };
    }
    
    public static PagingStreamingData<T> Create(PagingAdditionalMetadata additionalMetadata, IAsyncEnumerable<T> items, PagingMetadata pagingMetadata)
    {
        LazyTask<PagingMetadata> GetMetadata() => LazyTask<PagingMetadata>.FromResult(pagingMetadata);
        LazyTask<IAsyncEnumerable<T>> GetItems() => LazyTask<IAsyncEnumerable<T>>.FromResult(items);
        LazyTask<PagingAdditionalMetadata> GetQueryExecutionTimeMilliseconds() => LazyTask<PagingAdditionalMetadata>.FromResult(additionalMetadata);

        return new PagingStreamingData<T>
        {
            Metadata = GetMetadata(),
            Items = GetItems(),
            AdditionalMetadata = GetQueryExecutionTimeMilliseconds(),
        };
    }
    
    public static PagingStreamingData<T> Create(HttpResponseMessage response, JsonSerializerOptions jsonSerializerOptions, CancellationToken ct)
    {
        var streamingJsonContext = new StreamingJsonContext(response);

        var getMetadata = GetMetadata();
        var getItems = GetItems();
        var getQueryExecutionTimeMilliseconds = GetQueryExecutionTimeMilliseconds();
        
        async LazyTask<PagingMetadata> GetMetadata()
        {
            var stream = await streamingJsonContext.ReadJsonAsync(ct);
            var result = await JsonSerializer.DeserializeAsync<PagingMetadata>(stream, jsonSerializerOptions, ct);
            return result!;
        }
        async LazyTask<IAsyncEnumerable<T>> GetItems()
        {
            await getMetadata;
            
            var stream = await streamingJsonContext.ReadJsonAsync(ct);
            return new OneTimeEnumerable<T>(JsonSerializer.DeserializeAsyncEnumerable<T>(stream, jsonSerializerOptions, ct)!);
        }
        async LazyTask<PagingAdditionalMetadata> GetQueryExecutionTimeMilliseconds()
        {
            await getMetadata;
            var items = (await getItems as OneTimeEnumerable<T>)!;
            await items.Enumeration.Task;
            
            var stream = await streamingJsonContext.ReadJsonAsync(ct);
            var result = await JsonSerializer.DeserializeAsync<PagingAdditionalMetadata>(stream, jsonSerializerOptions, ct);
            await streamingJsonContext.DisposeAsync();
            return result!;
        }
        
        return new PagingStreamingData<T>
        {
            Metadata = getMetadata,
            Items = getItems,
            AdditionalMetadata = getQueryExecutionTimeMilliseconds,
        };
    }
    
    public required LazyTask<PagingMetadata> Metadata { get; init; }
    public required LazyTask<IAsyncEnumerable<T>> Items { get; init; }
    public required LazyTask<PagingAdditionalMetadata> AdditionalMetadata { get; init; }
}