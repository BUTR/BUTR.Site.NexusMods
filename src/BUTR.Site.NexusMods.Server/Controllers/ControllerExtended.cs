using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;
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
    protected StreamingJsonActionResult StreamingJson(IEnumerable<Func<Stream, CancellationToken, Task>> contents, string mime = "application/x-ndjson")
    {
        return new StreamingJsonActionResult(contents, mime);
    }

    [NonAction]
    protected ActionResult<APIResponse<PagingData<TResult>?>> APIPagingResponse<TResult, TSource>(Paging<TSource> data, Func<IAsyncEnumerable<TSource>, IAsyncEnumerable<TResult>> func)
        where TResult : class
        where TSource : class
    {
        return APIResponse(PagingData<TResult>.Create<TSource>(data, func));
    }

    [NonAction]
    protected StreamingJsonActionResult APIPagingResponseStreaming<TResult, TSource>(Paging<TSource> data, Func<IAsyncEnumerable<TSource>, IAsyncEnumerable<TResult>> func)
        where TResult : class
        where TSource : class
    {
        var options = HttpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>().Value;
        return StreamingJson(new Func<Stream, CancellationToken, Task>[]
        {
            async (stream, ct_) =>
            {
                await JsonSerializer.SerializeAsync(stream, new APIStreamingResponse(string.Empty), options.JsonSerializerOptions, ct_);
            },
            async (stream, ct_) =>
            {
                await JsonSerializer.SerializeAsync(stream, data.Metadata, options.JsonSerializerOptions, ct_);
            },
            async (stream, ct_) =>
            {
                await JsonSerializer.SerializeAsync(stream, func(data.Items), options.JsonSerializerOptions, ct_);
            },
            async (stream, ct_) =>
            {
                await JsonSerializer.SerializeAsync(stream, new
                {
                    QueryExecutionTimeMilliseconds = Stopwatch.GetElapsedTime(data.StartTime).Milliseconds
                }, options.JsonSerializerOptions, ct_);
            }
        }, "application/x-ndjson-butr-paging");
    }

    [NonAction]
    protected ActionResult<APIResponse<PagingData<TResult>?>> APIPagingResponse<TResult>(Paging<TResult> data, Func<IAsyncEnumerable<TResult>, IAsyncEnumerable<TResult>> func)
        where TResult : class
    {
        return APIResponse(PagingData<TResult>.Create(data, func));
    }

    [NonAction]
    protected ActionResult<APIResponse<PagingData<TResult>?>> APIPagingResponse<TResult>(Paging<TResult> paginated)
        where TResult : class
    {
        return APIResponse(PagingData<TResult>.Create(paginated));
    }

    [NonAction]
    protected ActionResult<APIResponse<T?>> APIResponse<T>([ActionResultObjectValue] T? value) => Models.API.APIResponse.From(value);
    [NonAction]
    protected ActionResult<APIResponse<T?>> APIResponseError<T>(string error) => Models.API.APIResponse.Error<T>(error);
}