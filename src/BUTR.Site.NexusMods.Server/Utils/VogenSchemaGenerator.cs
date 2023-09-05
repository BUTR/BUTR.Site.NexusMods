using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

using System;
using System.Reflection;

using Vogen;

namespace BUTR.Site.NexusMods.Server.Utils;

public class VogenSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.GetCustomAttribute<ValueObjectAttribute<string>>() is not null)
        {
            schema.Type = "string";
            schema.Format = "";
        }
        if (context.Type.GetCustomAttribute<ValueObjectAttribute<byte>>() is not null)
        {
            schema.Type = "integer";
            schema.Format = "";
        }
        if (context.Type.GetCustomAttribute<ValueObjectAttribute<int>>() is not null)
        {
            schema.Type = "integer";
            schema.Format = "";
        }
        if (context.Type.GetCustomAttribute<ValueObjectAttribute<Guid>>() is not null)
        {
            schema.Type = "string";
            schema.Format = "uuid";
        }
    }
}