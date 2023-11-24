namespace BUTR.Site.NexusMods.Server.Utils;

public static class QueryableHelper
{
    public static Type ConvertValueObject(Type type)
    {
        if (type.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IValueObject<>)) is { } vogen)
        {
            if (vogen.GetGenericArguments() is [{ } valueObject])
                type = valueObject;
        }

        return type;
    }
}