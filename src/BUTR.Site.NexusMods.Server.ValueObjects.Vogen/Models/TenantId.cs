namespace BUTR.Site.NexusMods.Server.Models;

using TType = TenantId;
using TValueType = Byte;

[TypeConverter(typeof(VogenTypeConverter<TType, TValueType>))]
[JsonConverter(typeof(VogenJsonConverter<TType, TValueType>))]
[ValueObject<TValueType>(conversions: Conversions.None, deserializationStrictness: DeserializationStrictness.AllowKnownInstances)]
public readonly partial record struct TenantId : IVogen<TType, TValueType>, IVogenParsable<TType, TValueType>, IVogenSpanParsable<TType, TValueType>, IVogenUtf8SpanParsable<TType, TValueType>, IHasDefaultValue<TType>
{
    public static readonly TType None = From(0);
    public static readonly TType Bannerlord = From(TenantUtils.BannerlordId);
    public static readonly TType Rimworld = From(TenantUtils.RimworldId);
    public static readonly TType StardewValley = From(TenantUtils.StardewValleyId);
    
    public static TType DefaultValue => None;

    public static IEnumerable<TType> Values
    {
        get
        {
            yield return Bannerlord;
            //yield return Rimworld;
            //yield return StardewValley;
            //yield return DarkestDungeon;
        }
    }
    
    public static TType Copy(TType instance) => instance with { };
    public static bool IsInitialized(TType instance) => instance._isInitialized;
    public static TType DeserializeDangerous(TValueType instance) => Deserialize(instance);

    public static TType FromTenant(int tenant)
    {
        foreach (var tenantId in Values)
        {
            if (tenantId.Value == tenant)
                return tenantId;
        }

        return None;
    }

    public NexusModsGameDomain ToGameDomain() =>
        TenantUtils.FromTenantToGameDomain(Value) is { } gameDomainRaw ? NexusModsGameDomain.FromGameDomain(gameDomainRaw) : NexusModsGameDomain.None;

    public string ToName() => TenantUtils.FromTenantToName(Value) ?? string.Empty;

    public static int GetHashCode(TType instance) => VogenDefaults<TType, TValueType>.GetHashCode(instance);

    public static bool Equals(TType left, TType right) => VogenDefaults<TType, TValueType>.Equals(left, right);
    public static bool Equals(TType left, TType right, IEqualityComparer<TType> comparer) => VogenDefaults<TType, TValueType>.Equals(left, right, comparer);

    public static int CompareTo(TType left, TType right) => VogenDefaults<TType, TValueType>.CompareTo(left, right);
}