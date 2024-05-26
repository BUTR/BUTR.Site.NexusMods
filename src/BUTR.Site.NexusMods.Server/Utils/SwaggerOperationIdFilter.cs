using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

using System.Collections.Generic;
using System.Linq;

namespace BUTR.Site.NexusMods.Server.Utils;

public class SwaggerOperationIdFilter : IOperationFilter
{
    private readonly Dictionary<string, string> _swaggerOperationIds = new Dictionary<string, string>();

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!(context.ApiDescription.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor))
        {
            return;
        }

        if (_swaggerOperationIds.TryGetValue(controllerActionDescriptor.Id, out var id))
        {
            operation.OperationId = id;
        }
        else
        {
            var operationIdBaseName = $"{controllerActionDescriptor.ControllerName}_{controllerActionDescriptor.ActionName}";
            var operationId = operationIdBaseName;
            var suffix = 2;
            while (_swaggerOperationIds.Values.Contains(operationId))
            {
                operationId = $"{operationIdBaseName}{suffix++}";
            }

            _swaggerOperationIds[controllerActionDescriptor.Id] = operationId;
            operation.OperationId = operationId;
        }
    }
}