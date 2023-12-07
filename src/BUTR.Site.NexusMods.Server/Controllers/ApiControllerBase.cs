using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;
using BUTR.Site.NexusMods.Server.Utils.Http.StreamingMultipartResults;

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

public class ApiControllerBase : ControllerBase
{
    [NonAction]
    protected ApiResult<PagingData<TResult>?> ApiPagingResult<TResult, TSource>(Paging<TSource> data, Func<IAsyncEnumerable<TSource>, IAsyncEnumerable<TResult>> func)
        where TResult : class
        where TSource : class
    {
        return ApiResult(PagingData<TResult>.Create(data, func));
    }

    [NonAction]
    protected StreamingMultipartResult ApiPagingStreamingResult<TResult, TSource>(Paging<TSource> data, Func<IAsyncEnumerable<TSource>, IAsyncEnumerable<TResult>> func)
        where TResult : class
        where TSource : class
    {
        var options = HttpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>().Value;
        IEnumerable<Func<Stream, CancellationToken, Task>> GetContent()
        {
            yield return (stream, ct_) => JsonSerializer.SerializeAsync(stream, Utils.Http.ApiResults.ApiResult.FromError(null), options.JsonSerializerOptions, ct_);
            yield return (stream, ct_) => JsonSerializer.SerializeAsync(stream, data.Metadata, options.JsonSerializerOptions, ct_);
            yield return (stream, ct_) => JsonSerializer.SerializeAsync(stream, func(data.Items), options.JsonSerializerOptions, ct_);
            yield return (stream, ct_) => JsonSerializer.SerializeAsync(stream, new PagingAdditionalMetadata
            {
                QueryExecutionTimeMilliseconds = (uint) Stopwatch.GetElapsedTime(data.StartTime).Milliseconds
            }, options.JsonSerializerOptions, ct_);
        }

        return new StreamingMultipartResult(GetContent(), "application/x-ndjson-butr-paging");
    }

    [NonAction]
    protected ApiResult<PagingData<TResult>?> ApiPagingResult<TResult>(Paging<TResult> paginated)
        where TResult : class
    {
        return ApiResult(PagingData<TResult>.Create(paginated));
    }

    [NonAction]
    protected ApiResult<T?> ApiResult<T>([ActionResultObjectValue] T? value) => Utils.Http.ApiResults.ApiResult<T>.FromResult(value);

    [NonAction]
    protected ApiResult ApiResultError(string error) => Utils.Http.ApiResults.ApiResult.FromError(new ProblemDetails
    {
        Detail = error,
    });

    [NonAction]
    protected ApiResult ApiResultError(int statusCode) => Utils.Http.ApiResults.ApiResult.FromError(new ProblemDetails
    {
        Status = statusCode,
    });

    [NonAction]
    protected ApiResult ApiResultError(string error, int statusCode) => Utils.Http.ApiResults.ApiResult.FromError(new ProblemDetails
    {
        Detail = error,
        Status = statusCode,
    });
}