using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;

public sealed class ApiResultOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext? context)
    {
        if (context == default || context.MethodInfo == default)
            return;

        if (!ApiResultUtils.IsReturnTypeApiResult(context.MethodInfo))
            return;

        if (!operation.Responses.TryGetValue("200", out var successResponse))
            return;
        
        var copy400 = CopyHelper.CopyPublicProperties(successResponse, new OpenApiResponse());
        copy400.Description = "Invalid API Request.";
        operation.Responses.Add("400", copy400);

        var copy500 = CopyHelper.CopyPublicProperties(successResponse, new OpenApiResponse());
        copy500.Description = "API Request Execution Error.";
        operation.Responses.Add("500", copy500);

        foreach (var (_, response) in operation.Responses)
        {
            response.Content.Remove("text/plain");
            response.Content.Remove("text/json");
        }
    }
}