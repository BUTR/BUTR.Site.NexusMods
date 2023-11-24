namespace BUTR.Site.NexusMods.Server.Models;

using TType = NexusModsModId;
using TValueType = Int32;

[TypeConverter(typeof(VogenTypeConverter<TType, TValueType>))]
[JsonConverter(typeof(VogenJsonConverter<TType, TValueType>))]
[ValueObject<TValueType>(conversions: Conversions.None)]
public readonly partial record struct NexusModsModId : IVogen<TType, TValueType>, IVogenParsable<TType, TValueType>, IVogenSpanParsable<TType, TValueType>, IVogenUtf8SpanParsable<TType, TValueType>
{
    public static readonly TType None = From(0);
    
    public static TType DefaultValue => None;
    
    public static TType Copy(TType instance) => instance with { };
    public static bool IsInitialized(TType instance) => instance._isInitialized;
    public static TType DeserializeDangerous(TValueType instance) => Deserialize(instance);

    public static bool TryParseUrl(string urlRaw, out TType modId)
    {
        modId = DefaultValue;

        if (!Uri.TryCreate(urlRaw, UriKind.Absolute, out var url))
            return false;

        if (!url.Host.EndsWith("nexusmods.com"))
            return false;

        if (url.LocalPath.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) is not [_, _, var modIdRaw, ..])
            return false;

        return TryParse(modIdRaw, out modId);
    }

    public static int GetHashCode(TType instance) => VogenDefaults<TType, TValueType>.GetHashCode(instance);

    public static bool Equals(TType left, TType right) => VogenDefaults<TType, TValueType>.Equals(left, right);
    public static bool Equals(TType left, TType right, IEqualityComparer<TType> comparer) => VogenDefaults<TType, TValueType>.Equals(left, right, comparer);

    public static int CompareTo(TType left, TType right) => VogenDefaults<TType, TValueType>.CompareTo(left, right);
}