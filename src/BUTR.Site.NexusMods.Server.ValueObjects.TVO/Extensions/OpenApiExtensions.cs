namespace BUTR.Site.NexusMods.Server.Models;

public static class OpenApiExtensions
{
    public static void ValueObjectFilter(this SwaggerGenOptions opt)
    {
        opt.SchemaFilter<TransparentValueObjectSchemaFilter>();
    }
}