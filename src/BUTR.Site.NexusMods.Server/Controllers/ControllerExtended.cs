using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

using System;
using System.Collections.Generic;

namespace BUTR.Site.NexusMods.Server.Controllers;

public class ControllerExtended : ControllerBase
{
    [NonAction]
    protected ActionResult<APIResponse<PagingData<TResult>?>> APIPagingResponse<TResult, TSource>(Paging<TSource> data, Func<IAsyncEnumerable<TSource>, IAsyncEnumerable<TResult>> func)
        where TResult : class
        where TSource : class
    {
        return APIResponse(PagingData<TResult>.Create<TSource>(data, func));
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