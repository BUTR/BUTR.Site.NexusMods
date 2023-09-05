using System.Collections.Generic;
using System.Linq;

namespace BUTR.Site.NexusMods.Shared.Helpers;

public static class TenantUtils
{
    public const string Bannerlord = "Bannerlord";
    public const string BannerlordId = "1";
    public const string BannerlordName = "Mount & Blade II: Bannerlord";
    public const string BannerlordGameDomain = "mountandblade2bannerlord";
    public const string Rimworld = "Rimworld";
    public const string RimworldId = "2";
    public const string RimworldName = "Rimworld";
    public const string RimworldGameDomain = "rimworld";
    public const string StardewValley = "StardewValley";
    public const string StardewValleyId = "3";
    public const string StardewValleyName = "Stardew Valley";
    public const string StardewValleyGameDomain = "stardewvalley";

    private sealed record TenantMetadata(string Id, string NexusModsId, string Name);

    private static readonly List<TenantMetadata> TenantMetadatas = new()
    {
        new(BannerlordId, BannerlordGameDomain, BannerlordName),
        new(RimworldId, RimworldGameDomain, RimworldName),
        new(StardewValleyId, StardewValleyGameDomain, StardewValleyName),
    };

    public static string? FromGameDomainToTenant(string gameDomain) => TenantMetadatas.FirstOrDefault(x => x.NexusModsId == gameDomain)?.Id;
    public static string? FromTenantToGameDomain(string tenant) => TenantMetadatas.FirstOrDefault(x => x.Id == tenant)?.NexusModsId;
    public static string? FromTenantToName(string tenant) => TenantMetadatas.FirstOrDefault(x => x.Id == tenant)?.Name;
}