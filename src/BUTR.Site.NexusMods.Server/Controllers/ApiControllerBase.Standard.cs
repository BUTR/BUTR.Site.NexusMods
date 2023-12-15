using BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System;

namespace BUTR.Site.NexusMods.Server.Controllers;

partial class ApiControllerBase
{
    [NonAction]
    protected ApiResult<T?> ApiOk<T>(T? value) => ApiResult(value);

    [NonAction]
    protected ApiResult ApiNoContent() => ApiResult();

    [NonAction]
    protected ApiResultCreated<T?> ApiCreated<T>(Uri locationUri, T? value) => ApiResultCreated(locationUri, value);

    [NonAction]
    protected ApiResultCreated<T?> ApiCreated<T>(string? actionName, T? value) => ApiResultCreated(actionName, null, null, value);

    [NonAction]
    protected ApiResultCreated<T?> ApiCreated<T>(string? actionName, object? routeValues, T? value) => ApiResultCreated(actionName, null, routeValues, value);

    [NonAction]
    protected ApiResultCreated<T?> ApiCreated<T>(string? actionName, string? controllerName, object? routeValues, T? value) =>
        ApiResultCreated(actionName, controllerName, routeValues, value);

    [NonAction]
    protected ApiResultAccepted<T?> ApiAccepted<T>(Uri locationUri, T? value) => ApiResultAccepted(locationUri, value);

    [NonAction]
    protected ApiResultAccepted<T?> ApiAccepted<T>(string? actionName, T? value) => ApiResultAccepted(actionName, null, null, value);

    [NonAction]
    protected ApiResultAccepted<T?> ApiAccepted<T>(string? actionName, object? routeValues, T? value) => ApiResultAccepted(actionName, null, routeValues, value);

    [NonAction]
    protected ApiResultAccepted<T?> ApiAccepted<T>(string? actionName, string? controllerName, object? routeValues, T? value) =>
        ApiResultAccepted(actionName, controllerName, routeValues, value);

    [NonAction]
    protected ApiResult ApiBadRequest(string error) => ApiResultError(error, StatusCodes.Status400BadRequest);

    [NonAction]
    protected ApiResult ApiForbid(string error) => ApiResultError(error, StatusCodes.Status403Forbidden);

    [NonAction]
    protected ApiResult ApiNotFound(string error) => ApiResultError(error, StatusCodes.Status404NotFound);

    [NonAction]
    protected ApiResult ApiConflict(string error) => ApiResultError(error, StatusCodes.Status409Conflict);

    [NonAction]
    protected ApiResult ApiUnprocessableEntity(string error) => ApiResultError(error, StatusCodes.Status422UnprocessableEntity);
}