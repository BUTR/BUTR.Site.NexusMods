using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;

using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;

public sealed class ApiResultProblemDetailsWriter : IProblemDetailsWriter
{
    public async ValueTask WriteAsync(ProblemDetailsContext context)
    {
        await context.HttpContext.Response.WriteAsJsonAsync(ApiResult.FromError(context.ProblemDetails));
    }

    public bool CanWrite(ProblemDetailsContext problemDetailsContext)
    {
        var endpoint = problemDetailsContext.HttpContext.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>() is { } context)
            return ApiResultUtils.IsReturnTypeApiResult(context.MethodInfo);

        return false;
    }
}