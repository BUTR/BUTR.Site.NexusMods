using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

using System.Linq;

namespace BUTR.Site.NexusMods.Server.Utils;

public class AuthResponsesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext? context)
    {
        if (context == default || context.MethodInfo == default || context.MethodInfo.DeclaringType == default)
            return;
        
        var anonymousAttributes = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
            .Union(context.MethodInfo.GetCustomAttributes(true))
            .OfType<AllowAnonymousAttribute>();
        if (anonymousAttributes.Any())
            return;
            
        var authAttributes = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
            .Union(context.MethodInfo.GetCustomAttributes(true))
            .OfType<AuthorizeAttribute>();
        if (!authAttributes.Any())
            return;

        if (operation.Responses.All(r => r.Key != "401"))
            operation.Responses.Add("401", new OpenApiResponse { Description = "User not authenticated." });

        if (operation.Responses.All(r => r.Key != "403"))
            operation.Responses.Add("403", new OpenApiResponse { Description = "User not authorized to access this endpoint." });
    }
}