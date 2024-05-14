namespace BUTR.Site.NexusMods.Server.Models;

using TType = NexusModsApiKey;
using TValueType = String;

[ValueObject<TValueType>(conversions: Conversions.SystemTextJson | Conversions.TypeConverter)]
public readonly partial struct NexusModsApiKey : IVogen<TType, TValueType>, IHasDefaultValue<TType>
{
    public static readonly TType None = From(string.Empty);

    public static NexusModsApiKey DefaultValue => None;

    public static TType Copy(TType instance) => instance with { };
    public static TType DeserializeDangerous(TValueType instance) => __Deserialize(instance);

    public static int GetHashCode(TType instance) => VogenDefaults<TType, TValueType>.GetHashCode(instance);

    public static bool Equals(TType left, TType right) => VogenDefaults<TType, TValueType>.Equals(left, right);
    public static bool Equals(TType left, TType right, IEqualityComparer<TType> comparer) => VogenDefaults<TType, TValueType>.Equals(left, right, comparer);

    public static int CompareTo(TType left, TType right) => VogenDefaults<TType, TValueType>.CompareTo(left, right);
}