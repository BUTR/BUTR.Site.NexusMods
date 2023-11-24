namespace BUTR.Site.NexusMods.Server.ValueObjects.Utils;

public sealed class TransparentValueObjectSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IValueObject<>)) is not { } vogen)
            return;

        if (vogen.GetGenericArguments() is not [{ } valueObject])
            return;

        var schemaValueObject = context.SchemaGenerator.GenerateSchema(valueObject, context.SchemaRepository, context.MemberInfo, context.ParameterInfo);
        CopyHelper.CopyPublicProperties(schemaValueObject, schema);
    }
}