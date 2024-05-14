namespace BUTR.Site.NexusMods.Server.ValueObjects.Utils;

public static class VogenUtils
{
    private static readonly Dictionary<Type, object?> _delegates = new();

    public static bool TryGetVogenDefaultValue(Type vogenType, out object? defaultValue)
    {
        if (_delegates.TryGetValue(vogenType, out defaultValue))
            return true;

        if (vogenType.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IVogen<,>)) is { } vogen)
        {
            var genericArgs = vogen.GenericTypeArguments;
            if (vogenType.IsAssignableTo(typeof(IHasDefaultValue<>)))
            {
                defaultValue = typeof(VogenUtils).GetMethod("GetDefaultValue", BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(genericArgs[0], genericArgs[1]).Invoke(null, null);
                _delegates.TryAdd(vogenType, defaultValue);
                return true;
            }
            defaultValue = typeof(VogenUtils).GetMethod("GetValue", BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(genericArgs[0], genericArgs[1]).Invoke(null, null);
            _delegates.TryAdd(vogenType, defaultValue);
            return true;
        }

        defaultValue = null;
        return false;
    }

    private static TVogen GetDefaultValue<TVogen, TValueObject>() where TVogen : IVogen<TVogen, TValueObject>, IHasDefaultValue<TVogen> => TVogen.DefaultValue;
    private static TVogen GetValue<TVogen, TValueObject>() where TVogen : IVogen<TVogen, TValueObject> => TVogen.From(default!);


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