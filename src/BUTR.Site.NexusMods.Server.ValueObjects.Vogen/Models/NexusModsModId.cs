namespace BUTR.Site.NexusMods.Server.Models;

using TType = NexusModsModId;
using TValueType = Int32;

[ValueObject<TValueType>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson | Conversions.TypeConverter)]
public readonly partial struct NexusModsModId : IVogen<TType, TValueType>, IHasDefaultValue<TType>
{
    public static readonly TType None = From(0);

    public static TType DefaultValue => None;

    public static TType Copy(TType instance) => instance with { };

    public static bool TryParseUrl(string? urlRaw, out TType modId)
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