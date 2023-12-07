using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

using System;

namespace BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;

public static class IMvcBuilderExtensions
{
    public static IMvcBuilder HandleInvalidModelStateError(this IMvcBuilder builder)
    {
        builder.ConfigureApiBehaviorOptions(options =>
        {
            var oldFactory = options.InvalidModelStateResponseFactory;
            options.InvalidModelStateResponseFactory = context => Factory(context, oldFactory);
        });

        return builder;
    }

    /// <summary>
    /// If the return type of the method is APIResponse, we use our standard return model
    /// </summary>
    private static IActionResult Factory(ActionContext actionContext, Func<ActionContext, IActionResult> oldFactory)
    {
        if (actionContext.ActionDescriptor is not ControllerActionDescriptor controllerActionDescriptor) return oldFactory(actionContext);
        if (!ApiResultUtils.IsReturnTypeApiResult(controllerActionDescriptor.MethodInfo)) return oldFactory(actionContext);

        var problemDetailsFactory = actionContext.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var problemDetails = problemDetailsFactory.CreateValidationProblemDetails(actionContext.HttpContext, actionContext.ModelState);
        return (ObjectResult) ApiResult.FromError(problemDetails);
    }
}