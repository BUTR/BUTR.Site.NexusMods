namespace BUTR.Site.NexusMods.Server.Models;

using TType = NexusModsUserEMail;
using TValueType = String;

[ValueObject<TValueType>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson | Conversions.TypeConverter)]
public readonly partial struct NexusModsUserEMail : IVogen<TType, TValueType>, IHasDefaultValue<TType>
{
    public static readonly TType Empty = From(string.Empty);

    public static TType DefaultValue => Empty;

    public static TType Copy(TType instance) => instance with { };

    public static int GetHashCode(TType instance) => VogenDefaults<TType, TValueType>.GetHashCode(instance);

    public static bool Equals(TType left, TType right) => VogenDefaults<TType, TValueType>.Equals(left, right);
    public static bool Equals(TType left, TType right, IEqualityComparer<TType> comparer) => VogenDefaults<TType, TValueType>.Equals(left, right, comparer);

    public static int CompareTo(TType left, TType right) => VogenDefaults<TType, TValueType>.CompareTo(left, right);
}