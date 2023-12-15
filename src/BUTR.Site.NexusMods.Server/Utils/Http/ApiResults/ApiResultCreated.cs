using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System;

namespace BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;

public sealed record ApiResultCreated<TValue> : ApiResult<TValue>
{
    public static implicit operator ApiResultCreated<TValue?>(ApiResult apiResult) => new(null, null, null, null, default, null, apiResult.Error);

    public static ApiResultCreated<TValue?> FromResultAction(HttpContext httpContext, string? actionName, string? controllerName, object? routeValues, TValue? data, int? status = null)
    {
        return new ApiResultCreated<TValue?>(null, controllerName, actionName, routeValues, data, status, null);
    }
    public static ApiResultCreated<TValue?> FromResultLocationUri(HttpContext httpContext, Uri locationUri, TValue? data, int? status = null)
    {
        return new ApiResultCreated<TValue?>(locationUri, null, null, null, data, status, null);
    }
    
    public Uri? LocationUri { get; }
    public string? ControllerName { get; }
    public string? ActionName { get; }
    public object? RouteValues { get; }
    
    private ApiResultCreated(Uri? locationUri, string? controllerName, string? actionName, object? routeValues, TValue? value, int? status, ProblemDetails? error) : base(value, status, error)
    {
        LocationUri = locationUri;
        ControllerName = controllerName;
        ActionName = actionName;
        RouteValues = routeValues;
    }

    public override IActionResult Convert() => Error is not null
        ? new ObjectResult(ApiResultModel<TValue>.From(this))
        : LocationUri is not null
            ? new CreatedResult(LocationUri, ApiResultModel<TValue>.From(this))
            : new CreatedAtActionResult(ActionName, ControllerName, RouteValues, ApiResultModel<TValue>.From(this)) { StatusCode = Status };
}