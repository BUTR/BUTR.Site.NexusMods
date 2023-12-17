namespace BUTR.Site.NexusMods.Server.Models;

using TType = TenantId;
using TValueType = Byte;

[TypeConverter(type: typeof(TransparentValueObjectTypeConverter<TType, TValueType>))]
[ValueObject<TValueType>]
public readonly partial struct TenantId : IAugmentWith<DefaultValueAugment, JsonAugment, EfCoreAugment>, IValueObjectFrom<TType, TValueType>
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
}

public static class TenantIdExtension
{
    public static PropertyBuilder<TType> HasValueObjectConversion(this PropertyBuilder<TType> propertyBuilder) => propertyBuilder
        .HasConversion<TType.EfCoreValueConverter, TType.EfCoreValueComparer>();
}