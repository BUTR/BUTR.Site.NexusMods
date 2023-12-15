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

        foreach(var property in properties)
        {
            if (property.HasAttribute<JsonIgnoreAttribute>() || !property.HasAttribute<RequiredMemberAttribute>()) continue;

            var jsonName = property.Name;
            if (property.HasAttribute<JsonPropertyNameAttribute>())
            {
                jsonName = property.GetCustomAttribute<JsonPropertyNameAttribute>()!.Name;
            }

            var jsonKey = schema.Properties.Keys.SingleOrDefault(key =>
                string.Equals(key, jsonName, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(jsonKey)) continue;

            // Check nullability of non primitive types
            var primitive = schema.Properties[jsonKey].Type != null;
            if (!primitive)
            {
                // Do not mark nullableref types.
                // Ref types cannot be marked as nullable, so this would lead to them being non nullable.
                var nullabilityInfo = nullabilityContext.Create(property);
                if (nullabilityInfo.ReadState == NullabilityState.Nullable) continue;
            }

            schema.Required.Add(jsonKey);
        }
    }

}