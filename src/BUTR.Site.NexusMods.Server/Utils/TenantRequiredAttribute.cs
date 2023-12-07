using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Linq;

namespace BUTR.Site.NexusMods.Server.Utils;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class TenantRequiredAttribute : Attribute, IResourceFilter
{
    private sealed class NopFilter : IResourceFilter
    {
        void IResourceFilter.OnResourceExecuting(ResourceExecutingContext context) { }
        void IResourceFilter.OnResourceExecuted(ResourceExecutedContext context) { }
    }

    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var idxThis = context.Filters.IndexOf(this);
        var idxNotRequired = context.Filters.Select((x, i) => (x, i)).Where(x => x.x is TenantNotRequiredAttribute).Select(x => x.i).FirstOrDefault(-1);
        if (idxThis < idxNotRequired)
            return;

        for (var i = 0; i < context.Filters.Count; i++)
        {
            if (context.Filters[i] is TenantNotRequiredAttribute)
                context.Filters[i] = new NopFilter();
        }

        if (context.HttpContext.RequestServices.GetRequiredService<ITenantContextAccessor>() is not { } tenantContextAccessor)
            return;

        if (context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>() is not { } problemDetailsFactory)
            return;

        if (tenantContextAccessor.Current == TenantId.None)
        {
            var modelStateDictionary = new ModelStateDictionary();
            modelStateDictionary.AddModelError("Tenant", "The field tenant is invalid.");
            var validationProblemDetails = problemDetailsFactory.CreateValidationProblemDetails(context.HttpContext, modelStateDictionary);

            context.Result = new ObjectResult(ApiResult.FromError(validationProblemDetails));
        }
    }

    public void OnResourceExecuted(ResourceExecutedContext context) { }
}