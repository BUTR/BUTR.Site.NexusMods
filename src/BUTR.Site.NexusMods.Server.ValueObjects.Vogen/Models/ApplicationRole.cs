namespace BUTR.Site.NexusMods.Server.Models;

using TType = ApplicationRole;
using TValueType = String;

[ValueObject<TValueType>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson | Conversions.TypeConverter, deserializationStrictness: DeserializationStrictness.AllowKnownInstances)]
public partial struct ApplicationRole : IVogen<TType, TValueType>, IHasDefaultValue<TType>
{
    public static readonly TType Anonymous = From(ApplicationRoles.Anonymous);
    public static readonly TType User = From(ApplicationRoles.User);
    public static readonly TType Moderator = From(ApplicationRoles.Moderator);
    public static readonly TType Administrator = From(ApplicationRoles.Administrator);

    public static TType DefaultValue => Anonymous;

    public static TType Copy(TType instance) => instance with { };

    public static int GetHashCode(TType instance) => VogenDefaults<TType, TValueType>.GetHashCode(instance);

    public static bool Equals(TType left, TType right) => VogenDefaults<TType, TValueType>.Equals(left, right);
    public static bool Equals(TType left, TType right, IEqualityComparer<TType> comparer) => VogenDefaults<TType, TValueType>.Equals(left, right, comparer);

    public static int CompareTo(TType left, TType right) => VogenDefaults<TType, TValueType>.CompareTo(left, right);
}