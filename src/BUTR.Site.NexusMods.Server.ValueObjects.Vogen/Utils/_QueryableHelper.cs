namespace BUTR.Site.NexusMods.Server.Utils;

public static class QueryableHelper
{
    public static Type ConvertValueObject(Type type)
    {
        if (type.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IVogen<,>)) is { } vogen)
        {
            if (vogen.GetGenericArguments() is [_, { } valueObject])
                type = valueObject;
        }

        return type;
    }
}