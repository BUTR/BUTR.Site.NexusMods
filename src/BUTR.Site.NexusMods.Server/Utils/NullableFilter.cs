using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

using System;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

namespace BUTR.Site.NexusMods.Server.Utils;

public class NullableFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var nullabilityContext = new NullabilityInfoContext();
        var properties = context.Type.GetProperties();

        foreach (var property in properties)
        {
            var jsonName = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name;
            var jsonKey = schema.Properties.Keys.SingleOrDefault(key => string.Equals(key, jsonName, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(jsonKey)) continue;

            schema.Properties[jsonKey].Nullable = nullabilityContext.Create(property).ReadState switch
            {
                NullabilityState.Unknown => false,
                NullabilityState.NotNull => false,
                NullabilityState.Nullable => true,
                _ => false
            };

            schema.Required.Add(jsonKey);
        }
    }
}