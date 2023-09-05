using BUTR.Site.NexusMods.Shared.Helpers;

using System.Collections.Generic;

using Vogen;

namespace BUTR.Site.NexusMods.Server.Models;

[ValueObject<byte>(conversions: Conversions.Default | Conversions.EfCoreValueConverter | Conversions.SystemTextJson)]
[Instance("None", "0")]
[Instance(TenantUtils.Bannerlord, TenantUtils.BannerlordId)]
[Instance(TenantUtils.Rimworld, TenantUtils.RimworldId)]
[Instance(TenantUtils.StardewValley, TenantUtils.StardewValleyId)]
public readonly partial struct TenantId
{
    public static IEnumerable<TenantId> Values
    {
        get
        {
            yield return Bannerlord;
            //yield return Rimworld;
            //yield return StardewValley;
            //yield return DarkestDungeon;
        }
    }

    public static TenantId FromTenant(string? tenant)
    {
        foreach (var tenantId in Values)
        {
            if (tenantId.Value.ToString() == tenant)
                return tenantId;
        }

        return None;
    }

    public NexusModsGameDomain ToGameDomain() => NexusModsGameDomain.FromGameDomain(TenantUtils.FromTenantToGameDomain(Value.ToString()));

    public string ToName() => TenantUtils.FromTenantToName(Value.ToString()) ?? string.Empty;
}