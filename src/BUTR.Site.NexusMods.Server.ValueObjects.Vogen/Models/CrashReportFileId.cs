namespace BUTR.Site.NexusMods.Server.Models;

using TType = CrashReportFileId;
using TValueType = String;

[TypeConverter(typeof(VogenTypeConverter<TType, TValueType>))]
[JsonConverter(typeof(VogenJsonConverter<TType, TValueType>))]
[ValueObject<TValueType>(conversions: Conversions.None)]
public readonly partial record struct CrashReportFileId : IVogen<TType, TValueType>
{
    public static TType Copy(TType instance) => instance with { };
    public static bool IsInitialized(TType instance) => instance._isInitialized;
    public static TType DeserializeDangerous(TValueType instance) => Deserialize(instance);

    public static int GetHashCode(TType instance) => VogenDefaults<TType, TValueType>.GetHashCode(instance);

    public static bool Equals(TType left, TType right) => VogenDefaults<TType, TValueType>.Equals(left, right);
    public static bool Equals(TType left, TType right, IEqualityComparer<TType> comparer) => VogenDefaults<TType, TValueType>.Equals(left, right, comparer);

    public static int CompareTo(TType left, TType right) => VogenDefaults<TType, TValueType>.CompareTo(left, right);
}