using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;

using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;

public sealed class ApiResultProblemDetailsWriter : IProblemDetailsWriter
{
    private readonly IActionResultExecutor<ObjectResult> _actionResultExecutor;

    public ApiResultProblemDetailsWriter(IActionResultExecutor<ObjectResult> actionResultExecutor)
    {
        _actionResultExecutor = actionResultExecutor;
    }

    public async ValueTask WriteAsync(ProblemDetailsContext context)
    {
        var routeData = context.HttpContext.GetRouteData();
        var action = context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<ActionDescriptor>();
        if (action is null) return;
        var actionContext = new ActionContext(context.HttpContext, routeData, action);
        var result = ApiResult.FromError(context.HttpContext, context.ProblemDetails);
        await _actionResultExecutor.ExecuteAsync(actionContext, result.Convert());
    }

    public bool CanWrite(ProblemDetailsContext problemDetailsContext)
    {
        var endpoint = problemDetailsContext.HttpContext.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>() is { } context)
            return ApiResultUtils.IsReturnTypeApiResult(context.MethodInfo);

        return false;
    }
}