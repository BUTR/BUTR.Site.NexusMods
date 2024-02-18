using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace BUTR.Site.NexusMods.Server.Utils;

public class RequiredMemberFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var nullabilityContext = new NullabilityInfoContext();
        var properties = context.Type.GetProperties();

        foreach (var property in properties)
        {
            if (property.HasAttribute<JsonIgnoreAttribute>() || !property.HasAttribute<RequiredMemberAttribute>()) continue;

            var jsonName = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name;
            var jsonKey = schema.Properties.Keys.SingleOrDefault(key => string.Equals(key, jsonName, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(jsonKey)) continue;

            // Do not mark nullableref types.
            // Ref types cannot be marked as nullable, so this would lead to them being non nullable.
            if (schema.Properties[jsonKey].Type is null && nullabilityContext.Create(property).ReadState == NullabilityState.Nullable) continue;

            schema.Required.Add(jsonKey);
        }
    }

}