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
        LazyTask<APIStreamingResponse> GetStatusAsync() => LazyTask<APIStreamingResponse>.FromResult(new APIStreamingResponse(string.Empty));
        LazyTask<PagingMetadata> GetMetadataAsync() => LazyTask<PagingMetadata>.FromResult(PagingMetadata.Empty);
        LazyTask<IAsyncEnumerable<T>> GetItemsAsync() => LazyTask<IAsyncEnumerable<T>>.FromResult(AsyncEnumerable.Empty<T>());
        LazyTask<PagingAdditionalMetadata> GetQueryExecutionTimeMilliseconds() => LazyTask<PagingAdditionalMetadata>.FromResult(PagingAdditionalMetadata.Empty);

        return new PagingStreamingData<T>
        {
            Status = GetStatusAsync(),
            Metadata = GetMetadataAsync(),
            Items = GetItemsAsync(),
            AdditionalMetadata = GetQueryExecutionTimeMilliseconds(),
        };
    }

    public static PagingStreamingData<T> Create(PagingMetadata pagingMetadata, IAsyncEnumerable<T> items, PagingAdditionalMetadata additionalMetadata)
    {
        LazyTask<APIStreamingResponse> GetStatusAsync() => LazyTask<APIStreamingResponse>.FromResult(new APIStreamingResponse(string.Empty));
        LazyTask<PagingMetadata> GetMetadataAsync() => LazyTask<PagingMetadata>.FromResult(pagingMetadata);
        LazyTask<IAsyncEnumerable<T>> GetItemsAsync() => LazyTask<IAsyncEnumerable<T>>.FromResult(items);
        LazyTask<PagingAdditionalMetadata> GetQueryExecutionTimeMillisecondsAsync() => LazyTask<PagingAdditionalMetadata>.FromResult(additionalMetadata);

        return new PagingStreamingData<T>
        {
            Status = GetStatusAsync(),
            Metadata = GetMetadataAsync(),
            Items = GetItemsAsync(),
            AdditionalMetadata = GetQueryExecutionTimeMillisecondsAsync(),
        };
    }

    public static PagingStreamingData<T> Create(HttpResponseMessage response, JsonSerializerOptions jsonSerializerOptions, CancellationToken ct)
    {
        var streamingJsonContext = new StreamingJsonContext(response);

        var hasError = false;
        var getStatus = GetStatusAsync();
        var getMetadata = GetMetadataAsync();
        var getItems = GetItemsAsync();
        var getQueryExecutionTimeMilliseconds = GetQueryExecutionTimeMillisecondsAsync();

        async LazyTask<APIStreamingResponse> GetStatusAsync()
        {
            var stream = await streamingJsonContext.ReadLfSeparatedJsonAsync(ct);
            var result = await JsonSerializer.DeserializeAsync<APIStreamingResponse>(stream, jsonSerializerOptions, ct);
            hasError = result is null || !string.IsNullOrEmpty(result.HumanReadableError);
            return result!;
        }
        async LazyTask<PagingMetadata> GetMetadataAsync()
        {
            if (hasError) return PagingMetadata.Empty;

            await getStatus;

            var stream = await streamingJsonContext.ReadLfSeparatedJsonAsync(ct);
            var result = await JsonSerializer.DeserializeAsync<PagingMetadata>(stream, jsonSerializerOptions, ct);
            return result!;
        }
        async LazyTask<IAsyncEnumerable<T>> GetItemsAsync()
        {
            if (hasError) return AsyncEnumerable.Empty<T>();

            await getStatus;
            await getMetadata;

            var stream = await streamingJsonContext.ReadLfSeparatedJsonAsync(ct);
            return new OneTimeEnumerable<T>(JsonSerializer.DeserializeAsyncEnumerable<T>(stream, jsonSerializerOptions, ct)!);
        }
        async LazyTask<PagingAdditionalMetadata> GetQueryExecutionTimeMillisecondsAsync()
        {
            if (hasError) return PagingAdditionalMetadata.Empty;

            await getStatus;
            await getMetadata;
            var items = (await getItems as OneTimeEnumerable<T>)!;
            await items.Enumeration.Task;

            var stream = await streamingJsonContext.ReadLfSeparatedJsonAsync(ct);
            var result = await JsonSerializer.DeserializeAsync<PagingAdditionalMetadata>(stream, jsonSerializerOptions, ct);
            await streamingJsonContext.DisposeAsync();
            return result!;
        }

        return new PagingStreamingData<T>
        {
            Status = getStatus,
            Metadata = getMetadata,
            Items = getItems,
            AdditionalMetadata = getQueryExecutionTimeMilliseconds,
        };
    }

    public required LazyTask<APIStreamingResponse> Status { get; init; }
    public required LazyTask<PagingMetadata> Metadata { get; init; }
    public required LazyTask<IAsyncEnumerable<T>> Items { get; init; }
    public required LazyTask<PagingAdditionalMetadata> AdditionalMetadata { get; init; }
}