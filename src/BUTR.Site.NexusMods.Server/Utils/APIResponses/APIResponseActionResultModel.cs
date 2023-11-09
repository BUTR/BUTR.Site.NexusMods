using BUTR.Site.NexusMods.Server.Models.API;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Utils.APIResponses;

public sealed record APIResponseActionResultModel<TValue>(TValue? Value, ProblemDetails? Error) : APIResponse<TValue>(Value, Error), IActionResult
{
    public Task ExecuteResultAsync(ActionContext context)
    {
        var executor = context.HttpContext.RequestServices.GetRequiredService<APIResponseActionResultExecutor<TValue>>();
        return executor.ExecuteAsync(context, this);
    }
}