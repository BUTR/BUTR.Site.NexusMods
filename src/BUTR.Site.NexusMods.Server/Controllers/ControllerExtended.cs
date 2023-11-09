using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Utils.APIResponses;
using BUTR.Site.NexusMods.Server.Utils.Http.StreamingJson;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Controllers;

public class ControllerExtended : ControllerBase
{
    [NonAction]
    protected APIResponseActionResult<PagingData<TResult>?> APIPagingResponse<TResult, TSource>(Paging<TSource> data, Func<IAsyncEnumerable<TSource>, IAsyncEnumerable<TResult>> func)
        where TResult : class
        where TSource : class
    {
        return APIResponse(PagingData<TResult>.Create(data, func));
    }

    [NonAction]
    protected StreamingJsonActionResult APIPagingResponseStreaming<TResult, TSource>(Paging<TSource> data, Func<IAsyncEnumerable<TSource>, IAsyncEnumerable<TResult>> func)
        where TResult : class
        where TSource : class
    {
        var options = HttpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>().Value;
        IEnumerable<Func<Stream, CancellationToken, Task>> GetContent()
        {
            yield return (stream, ct_) => JsonSerializer.SerializeAsync(stream, new APIStreamingResponse(null), options.JsonSerializerOptions, ct_);
            yield return (stream, ct_) => JsonSerializer.SerializeAsync(stream, data.Metadata, options.JsonSerializerOptions, ct_);
            yield return (stream, ct_) => JsonSerializer.SerializeAsync(stream, func(data.Items), options.JsonSerializerOptions, ct_);
            yield return (stream, ct_) => JsonSerializer.SerializeAsync(stream, new PagingAdditionalMetadata
            {
                QueryExecutionTimeMilliseconds = (uint) Stopwatch.GetElapsedTime(data.StartTime).Milliseconds
            }, options.JsonSerializerOptions, ct_);
        }
        
        return new StreamingJsonActionResult(GetContent(), "application/x-ndjson-butr-paging");
    }

    [NonAction]
    protected APIResponseActionResult<PagingData<TResult>?> APIPagingResponse<TResult>(Paging<TResult> data, Func<IAsyncEnumerable<TResult>, IAsyncEnumerable<TResult>> func)
        where TResult : class
    {
        return APIResponse(PagingData<TResult>.Create(data, func));
    }

    [NonAction]
    protected APIResponseActionResult<PagingData<TResult>?> APIPagingResponse<TResult>(Paging<TResult> paginated)
        where TResult : class
    {
        return APIResponse(PagingData<TResult>.Create(paginated));
    }

    [NonAction]
    protected APIResponseActionResult<T?> APIResponse<T>([ActionResultObjectValue] T? value) => APIResponseActionResult<T>.FromResult(value);

    [NonAction]
    protected APIResponseActionResult<T?> APIResponseError<T>(string error) => APIResponseActionResult<T>.FromError(new ProblemDetails
    {
        Detail = error,
    });

    [NonAction]
    protected APIResponseActionResult<T?> APIResponseError<T>(int statusCode) => APIResponseActionResult<T>.FromError(new ProblemDetails
    {
        Status = statusCode,
    });

    [NonAction]
    protected APIResponseActionResult<T?> APIResponseError<T>(string error, int statusCode) => APIResponseActionResult<T>.FromError(new ProblemDetails
    {
        Detail = error,
        Status = statusCode,
    });
}