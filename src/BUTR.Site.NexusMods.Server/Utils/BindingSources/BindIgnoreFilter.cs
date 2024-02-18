using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

using System;
using System.Linq;

namespace BUTR.Site.NexusMods.Server.Utils.BindingSources;

public sealed class BindIgnoreFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var parametersToHide = context.ApiDescription.ParameterDescriptions
            .Where(ParameterHasIgnoreAttribute);

        foreach (var parameterToHide in parametersToHide)
        {
            var parameter = operation.Parameters.FirstOrDefault(parameter => parameter.Name.Equals(parameterToHide.Name, StringComparison.OrdinalIgnoreCase));
            operation.Parameters.Remove(parameter);
        }

    }

    private static bool ParameterHasIgnoreAttribute(ApiParameterDescription parameterDescription) =>
        parameterDescription.ModelMetadata is DefaultModelMetadata metadata && metadata.Attributes.ParameterAttributes?.Any(attribute => attribute is IBindIgnore) == true;
}