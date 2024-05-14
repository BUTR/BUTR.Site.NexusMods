using System;
using System.Collections.Generic;

namespace BUTR.Site.NexusMods.Shared.Helpers;

public static class TenantUtils
{
    public const string Bannerlord = "Bannerlord";
    public const int BannerlordId = 1;
    public const string BannerlordName = "Mount & Blade II: Bannerlord";
    public const string BannerlordGameDomain = "mountandblade2bannerlord";
    public const string Rimworld = "Rimworld";
    public const int RimworldId = 2;
    public const string RimworldName = "Rimworld";
    public const string RimworldGameDomain = "rimworld";
    public const string StardewValley = "StardewValley";
    public const int StardewValleyId = 3;
    public const string StardewValleyName = "Stardew Valley";
    public const string StardewValleyGameDomain = "stardewvalley";
    public const int ValheimId = 3;
    public const string ValheimName = "Valheim";
    public const string ValheimGameDomain = "valheim";

    private sealed record TenantMetadata(int Id, string NexusModsId, string Name);

    private static readonly List<TenantMetadata> TenantMetadatas = new()
    {
        new(BannerlordId, BannerlordGameDomain, BannerlordName),
        new(RimworldId, RimworldGameDomain, RimworldName),
        new(StardewValleyId, StardewValleyGameDomain, StardewValleyName),
        new(ValheimId, ValheimGameDomain, ValheimName),
    };

    public static int? FromGameDomainToTenant(string gameDomain) => TenantMetadatas.Find(x => string.Equals(x.NexusModsId, gameDomain, StringComparison.Ordinal))?.Id;
    public static string? FromTenantToGameDomain(int tenant) => TenantMetadatas.Find(x => x.Id == tenant)?.NexusModsId;
    public static string? FromTenantToName(int tenant) => TenantMetadatas.Find(x => x.Id == tenant)?.Name;
}