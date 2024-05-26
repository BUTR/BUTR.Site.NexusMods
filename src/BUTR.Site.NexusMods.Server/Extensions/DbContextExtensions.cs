using Microsoft.EntityFrameworkCore;

namespace BUTR.Site.NexusMods.Server.Extensions;

public static class DbContextExtensions
{
    public static void UpdateProperties(this DbContext context, object target, object source)
    {
        var targetEntry = context.Entry(target);
        foreach (var targetPropertyEntry in targetEntry.Properties)
        {
            var targetProperty = targetPropertyEntry.Metadata;

            // Skip shadow and key properties
            if (targetProperty.IsShadowProperty() || (targetEntry.IsKeySet && targetProperty.IsKey())) continue;
            targetPropertyEntry.CurrentValue = targetProperty.GetGetter().GetClrValue(source);
        }
    }
}