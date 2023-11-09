using Microsoft.AspNetCore.Mvc.ModelBinding;

using System;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Utils.BindingSources;

public class ClaimsValueProviderFactory : IValueProviderFactory
{
    public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.ValueProviders.Add(new ClaimsValueProvider(ClaimsBindingSource.BindingSource, context.ActionContext.HttpContext.User));

        return Task.CompletedTask;
    }
}