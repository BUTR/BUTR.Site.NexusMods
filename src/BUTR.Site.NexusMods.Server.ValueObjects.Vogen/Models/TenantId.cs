namespace BUTR.Site.NexusMods.Server.Models;

using TType = TenantId;
using TValueType = Byte;

[ValueObject<TValueType>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson | Conversions.TypeConverter, deserializationStrictness: DeserializationStrictness.AllowKnownInstances)]
public readonly partial struct TenantId : IHasDefaultValue<TType>
{
    public static readonly TType None = new(0);
    public static readonly TType Bannerlord = new(TenantUtils.BannerlordId);
    public static readonly TType Rimworld = new(TenantUtils.RimworldId);
    public static readonly TType StardewValley = new(TenantUtils.StardewValleyId);
    public static readonly TType Valheim = new(TenantUtils.ValheimId);
    public static readonly TType Error = new(255);

    public static TType DefaultValue => None;

    public static IEnumerable<TType> Values
    {
        get
        {
            yield return Bannerlord;
            //yield return Rimworld;
            //yield return StardewValley;
            //yield return Valheim;
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