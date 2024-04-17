using BUTR.CrashReport.Models;

namespace BUTR.Site.NexusMods.Server.Models;

using TType = ExceptionTypeId;
using TValueType = String;

[TypeConverter(typeof(VogenTypeConverter<TType, TValueType>))]
[JsonConverter(typeof(VogenJsonConverter<TType, TValueType>))]
[ValueObject<TValueType>(conversions: Conversions.None)]
public readonly partial record struct ExceptionTypeId : IVogen<TType, TValueType>
{
    public static TType Copy(TType instance) => instance with { };
    public static bool IsInitialized(TType instance) => instance._isInitialized;
    public static TType DeserializeDangerous(TValueType instance) => Deserialize(instance);

    public static TType FromException(ExceptionModel exception)
    {
        var exc = exception;
        while (exc.InnerException is not null)
            exc = exc.InnerException;

        return From(exc.Type);
    }
    public static bool TryParseFromException(TValueType exception, out TType value)
    {
        Span<Range> dest = stackalloc Range[32];
        ReadOnlySpan<char> lastTypeLine = default;
        foreach (ReadOnlySpan<char> line in exception.SplitLines())
        {
            var count = line.Split(dest, ':');
            if (count != 2) continue;
            var firstPart = line[dest[0]].Trim();
            var secondPart = line[dest[1]].Trim();
            if (firstPart is "Type")
                lastTypeLine = secondPart;
        }

        if (lastTypeLine.Length > 0)
        {
            value = From(lastTypeLine.ToString());
            return true;
        }

        value = From("");
        return false;
    }

    public static int GetHashCode(TType instance) => VogenDefaults<TType, TValueType>.GetHashCode(instance);

    public static bool Equals(TType left, TType right) => VogenDefaults<TType, TValueType>.Equals(left, right);
    public static bool Equals(TType left, TType right, IEqualityComparer<TType> comparer) => VogenDefaults<TType, TValueType>.Equals(left, right, comparer);

    public static int CompareTo(TType left, TType right) => VogenDefaults<TType, TValueType>.CompareTo(left, right);
}