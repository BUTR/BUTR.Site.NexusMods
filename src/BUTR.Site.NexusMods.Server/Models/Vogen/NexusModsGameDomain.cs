using BUTR.Site.NexusMods.Shared.Helpers;

using System;
using System.Collections.Generic;

using Vogen;

namespace BUTR.Site.NexusMods.Server.Models;

[ValueObject<string>(conversions: Conversions.Default | Conversions.EfCoreValueConverter | Conversions.SystemTextJson)]
[Instance("None", "")]
[Instance(TenantUtils.Bannerlord, TenantUtils.BannerlordGameDomain)]
[Instance(TenantUtils.Rimworld, TenantUtils.RimworldGameDomain)]
[Instance(TenantUtils.StardewValley, TenantUtils.StardewValleyGameDomain)]
public readonly partial struct NexusModsGameDomain
{
    public static IEnumerable<NexusModsGameDomain> Values
    {
        get
        {
            yield return Bannerlord;
            //yield return Rimworld;
            //yield return StardewValley;
        }
    }

    public static bool TryParse(string url, out NexusModsGameDomain gameDomain)
    {
        gameDomain = None;

        if (!url.Contains("nexusmods.com/"))
            return false;

        var str1 = url.Split("nexusmods.com/");
        if (str1.Length != 2)
            return false;

        var split = str1[1].Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (split.Length < 2)
            return false;

        gameDomain = From(split[0]);
        return true;
    }

    public static NexusModsGameDomain FromGameDomain(string? gameDomain)
    {
        foreach (var nexusModsGameDomain in Values)
        {
            if (nexusModsGameDomain.Value == gameDomain)
                return nexusModsGameDomain;
        }

        return None;
    }

    public TenantId ToTenant() => TenantId.FromTenant(TenantUtils.FromGameDomainToTenant(Value));
}