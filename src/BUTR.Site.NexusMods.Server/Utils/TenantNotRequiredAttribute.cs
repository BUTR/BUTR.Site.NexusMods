using Microsoft.AspNetCore.Mvc.Filters;

using System;
using System.Linq;

namespace BUTR.Site.NexusMods.Server.Utils;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class TenantNotRequiredAttribute : Attribute, IResourceFilter
{
    private sealed class NopFilter : IResourceFilter
    {
        void IResourceFilter.OnResourceExecuting(ResourceExecutingContext context) { }
        void IResourceFilter.OnResourceExecuted(ResourceExecutedContext context) { }
    }

    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var idxThis = context.Filters.IndexOf(this);
        var idxRequired = context.Filters.Select((x, i) => (x, i)).Where(x => x.x is TenantRequiredAttribute).Select(x => x.i).FirstOrDefault(-1);
        if (idxThis < idxRequired)
            return;

        for (var i = 0; i < context.Filters.Count; i++)
        {
            if (context.Filters[i] is TenantRequiredAttribute)
                context.Filters[i] = new NopFilter();
        }
    }

    public void OnResourceExecuted(ResourceExecutedContext context) { }
}