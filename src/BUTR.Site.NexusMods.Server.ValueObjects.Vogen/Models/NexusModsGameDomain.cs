namespace BUTR.Site.NexusMods.Server.Models;

using TType = NexusModsGameDomain;
using TValueType = String;

[ValueObject<TValueType>(conversions: Conversions.SystemTextJson | Conversions.TypeConverter, deserializationStrictness: DeserializationStrictness.AllowKnownInstances)]
public readonly partial struct NexusModsGameDomain : IHasDefaultValue<TType>
{
    public static readonly TType None = From(string.Empty);
    public static readonly TType Bannerlord = From(TenantUtils.BannerlordGameDomain);
    public static readonly TType Rimworld = From(TenantUtils.RimworldGameDomain);
    public static readonly TType StardewValley = From(TenantUtils.StardewValleyGameDomain);
    public static readonly TType Valheim = From(TenantUtils.ValheimGameDomain);

    public static TType DefaultValue => None;

    public static IEnumerable<TType> Values
    {
        get
        {
            yield return Bannerlord;
            yield return Rimworld;
            yield return StardewValley;
            yield return Valheim;
        }
    }

    public static bool TryParse(string urlRaw, out TType gameDomain)
    {
        gameDomain = DefaultValue;

        if (!Uri.TryCreate(urlRaw, UriKind.Absolute, out var url))
            return false;

        if (!url.Host.EndsWith("nexusmods.com"))
            return false;

        if (url.LocalPath.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) is not [var gameDomainStr, ..])
            return false;

        gameDomain = From(gameDomainStr);
        return true;
    }

    public static TType FromGameDomain(string gameDomain)
    {
        foreach (var nexusModsGameDomain in Values)
        {
            if (nexusModsGameDomain.Value == gameDomain)
                return nexusModsGameDomain;
        }

        return None;
    }

    public TenantId ToTenant() => TenantUtils.FromGameDomainToTenant(Value) is { } tenantRaw ? TenantId.FromTenant(tenantRaw) : TenantId.None;
}