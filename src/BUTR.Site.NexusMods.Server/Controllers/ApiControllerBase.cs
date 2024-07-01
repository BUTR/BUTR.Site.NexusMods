using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;
using BUTR.Site.NexusMods.Server.Utils.Http.StreamingMultipartResults;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;

namespace BUTR.Site.NexusMods.Server.Controllers;

public partial class ApiControllerBase : ControllerBase
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
            yield return (stream, ct_) => JsonSerializer.SerializeAsync(stream, Utils.Http.ApiResults.ApiResult.FromError(HttpContext, null), options.JsonSerializerOptions, ct_);
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
    protected ApiResult ApiResult(int statusCode = StatusCodes.Status204NoContent)
    {
        if (statusCode is < 100 or >= 400)
            throw new ArgumentOutOfRangeException(nameof(statusCode));

        return Utils.Http.ApiResults.ApiResult.FromResult(HttpContext, statusCode);
    }

    [NonAction]
    protected ApiResult<T?> ApiResult<T>([ActionResultObjectValue] T value) => Utils.Http.ApiResults.ApiResult<T>.FromResult(HttpContext, value);

    [NonAction]
    protected ApiResultCreated<T?> ApiResultCreated<T>(Uri locationUri, T value, int statusCode = StatusCodes.Status201Created)
    {
        if (statusCode is < 100 or >= 400)
            throw new ArgumentOutOfRangeException(nameof(statusCode));

        return Utils.Http.ApiResults.ApiResultCreated<T>.FromResultLocationUri(HttpContext, locationUri, value, statusCode);
    }

    [NonAction]
    protected ApiResultCreated<T?> ApiResultCreated<T>(string? actionName, string? controllerName, object? routeValues, T value, int statusCode = StatusCodes.Status201Created)
    {
        if (statusCode is < 100 or >= 400)
            throw new ArgumentOutOfRangeException(nameof(statusCode));

        return Utils.Http.ApiResults.ApiResultCreated<T>.FromResultAction(HttpContext, actionName, controllerName, routeValues, value, statusCode);
    }

    [NonAction]
    protected ApiResultAccepted<T?> ApiResultAccepted<T>(Uri locationUri, T value, int statusCode = StatusCodes.Status202Accepted)
    {
        if (statusCode is < 100 or >= 400)
            throw new ArgumentOutOfRangeException(nameof(statusCode));

        return Utils.Http.ApiResults.ApiResultAccepted<T>.FromResultLocationUri(HttpContext, locationUri, value, statusCode);
    }

    [NonAction]
    protected ApiResultAccepted<T?> ApiResultAccepted<T>(string? actionName, string? controllerName, object? routeValues, T value, int statusCode = StatusCodes.Status202Accepted)
    {
        if (statusCode is < 100 or >= 400)
            throw new ArgumentOutOfRangeException(nameof(statusCode));

        return Utils.Http.ApiResults.ApiResultAccepted<T>.FromResultAction(HttpContext, actionName, controllerName, routeValues, value, statusCode);
    }

    [NonAction]
    protected ApiResult ApiResultError(string error, int statusCode)
    {
        var routeAttribute = Url.ActionContext.ActionDescriptor.EndpointMetadata.OfType<HttpMethodAttribute>().First();
        var routeTemplate = routeAttribute.Template;
        
        var loggerFactory = HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(GetType());
        logger.LogError("Route: {Route}, API Error: {Error}", routeTemplate, error);
        
        if (statusCode is < 400 or >= 600)
            throw new ArgumentOutOfRangeException(nameof(statusCode));

        return Utils.Http.ApiResults.ApiResult.FromError(HttpContext, new ProblemDetails
        {
            Detail = error,
            Status = statusCode
        });
    }
}