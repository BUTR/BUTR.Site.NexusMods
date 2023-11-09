using BUTR.Site.NexusMods.Server.Models.API;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;

using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Utils.APIResponses;

public sealed class APIResponseProblemDetailsWriter : IProblemDetailsWriter
{
    public async ValueTask WriteAsync(ProblemDetailsContext context)
    {
        await context.HttpContext.Response.WriteAsJsonAsync(APIResponse.FromError<object>(context.ProblemDetails));
    }

    public bool CanWrite(ProblemDetailsContext context2)
    {
        if (context2.HttpContext.Features.Get<IExceptionHandlerFeature>()?.Endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>() is { } context)
            return APIResponseUtils.IsReturnTypeAPIResponse(context.MethodInfo);

        return false;
    }
}