using System.ComponentModel;

namespace BUTR.Site.NexusMods.Shared.Extensions;

public static class TenantExtensions
{
    public static string GameDomain(this Tenant value)
    {
        var description = value.ToString();
        var fieldInfo = value.GetType().GetField(description);

        if (fieldInfo == null) return description;

        var attrs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);
        if (attrs is { Length: > 0 })
            description = ((DescriptionAttribute)attrs[0]).Description;

        return description;
    }
}