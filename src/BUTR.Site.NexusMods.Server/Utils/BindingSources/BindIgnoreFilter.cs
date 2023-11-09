using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

using System.Linq;

namespace BUTR.Site.NexusMods.Server.Utils.BindingSources;

public sealed class BindIgnoreFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var parametersToHide = context.ApiDescription.ParameterDescriptions
            .Where(ParameterHasIgnoreAttribute)
            .ToList();

        if (parametersToHide.Count == 0)
            return;

        foreach (var parameterToHide in parametersToHide)
        {
            var parameter = operation.Parameters.FirstOrDefault(parameter => string.Equals(parameter.Name, parameterToHide.Name, System.StringComparison.OrdinalIgnoreCase));
            if (parameter != null)
            {
                operation.Parameters.Remove(parameter);
            }
        }

    }

    private static bool ParameterHasIgnoreAttribute(ApiParameterDescription parameterDescription)
    {
        if (parameterDescription.ModelMetadata is DefaultModelMetadata metadata)
        {
            return metadata.Attributes.ParameterAttributes?.Any(attribute => attribute.GetType().GetInterfaces().Any(x => x == typeof(IBindIgnore))) == true;
        }

        return false;
    }
}