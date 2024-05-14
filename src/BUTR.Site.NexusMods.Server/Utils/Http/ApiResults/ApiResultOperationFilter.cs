using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

using System.Reflection;

namespace BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;

public sealed class ApiResultOperationFilter : IOperationFilter
{
    private static T CopyPublicProperties<T>(T oldObject, T newObject) where T : class
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

        if (ReferenceEquals(oldObject, newObject)) return newObject;

        var type = typeof(T);
        var propertyList = type.GetProperties(flags);
        if (propertyList.Length <= 0) return newObject;

        foreach (var newObjProp in propertyList)
        {
            var oldProp = type.GetProperty(newObjProp.Name, flags)!;
            if (!oldProp.CanRead || !newObjProp.CanWrite) continue;

            var value = oldProp.GetValue(oldObject);
            newObjProp.SetValue(newObject, value);
        }

        return newObject;
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext? context)
    {
        if (context == default || context.MethodInfo == default)
            return;

        if (!ApiResultUtils.IsReturnTypeApiResult(context.MethodInfo))
            return;

        if (!operation.Responses.TryGetValue("200", out var successResponse))
            return;

        var copy400 = CopyPublicProperties(successResponse, new OpenApiResponse());
        copy400.Description = "Invalid API Request.";
        operation.Responses.Add("400", copy400);

        var copy500 = CopyPublicProperties(successResponse, new OpenApiResponse());
        copy500.Description = "API Request Execution Error.";
        operation.Responses.Add("500", copy500);

        foreach (var (_, response) in operation.Responses)
        {
            response.Content.Remove("text/plain");
            response.Content.Remove("text/json");
        }
    }
}