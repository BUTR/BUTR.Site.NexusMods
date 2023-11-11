using BUTR.Site.NexusMods.Server.Utils.Vogen;
using BUTR.Site.NexusMods.Shared.Helpers;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

using Vogen;

namespace BUTR.Site.NexusMods.Server.Models;

using TType = NexusModsGameDomain;
using TValueType = String;

[TypeConverter(typeof(VogenTypeConverter<TType, TValueType>))]
[JsonConverter(typeof(VogenJsonConverter<TType, TValueType>))]
[ValueObject<TValueType>(conversions: Conversions.None, deserializationStrictness: DeserializationStrictness.AllowKnownInstances)]
[Instance("None", "")]
[Instance(TenantUtils.Bannerlord, TenantUtils.BannerlordGameDomain)]
[Instance(TenantUtils.Rimworld, TenantUtils.RimworldGameDomain)]
[Instance(TenantUtils.StardewValley, TenantUtils.StardewValleyGameDomain)]
public readonly partial record struct NexusModsGameDomain : IVogen<TType, TValueType>, IHasDefaultValue<TType>
{
    public static TType Copy(TType instance) => instance with { };
    public static bool IsInitialized(TType instance) => instance._isInitialized;
    public static TType DeserializeDangerous(TValueType instance) => Deserialize(instance);
    public static TType DefaultValue => None;

    public static IEnumerable<TType> Values
    {
        get
        {
            yield return Bannerlord;
            yield return Rimworld;
            yield return StardewValley;
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


    public static int GetHashCode(TType instance) => VogenDefaults<TType, TValueType>.GetHashCode(instance);

    public static bool Equals(TType left, TType right) => VogenDefaults<TType, TValueType>.Equals(left, right);
    public static bool Equals(TType left, TType right, IEqualityComparer<TType> comparer) => VogenDefaults<TType, TValueType>.Equals(left, right, comparer);

    public static int CompareTo(TType left, TType right) => VogenDefaults<TType, TValueType>.CompareTo(left, right);
}