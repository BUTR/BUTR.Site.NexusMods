using System;
using System.Collections.Generic;

namespace BUTR.Site.NexusMods.Shared.Helpers;

public static class TenantUtils
{
    public const int BannerlordId = 1;
    public const string BannerlordName = "Mount & Blade II: Bannerlord";
    public const string BannerlordGameDomain = "mountandblade2bannerlord";
    public const int BannerlordNexusModsId = 3174;

    public const int RimworldId = 2;
    public const string RimworldName = "Rimworld";
    public const string RimworldGameDomain = "rimworld";
    public const int RimworldNexusModsId = 424;

    public const int StardewValleyId = 3;
    public const string StardewValleyName = "Stardew Valley";
    public const string StardewValleyGameDomain = "stardewvalley";
    public const int StardewValleyNexusModsId = 1303;

    public const int ValheimId = 3;
    public const string ValheimName = "Valheim";
    public const string ValheimGameDomain = "valheim";
    public const int ValheimNexusModsId = 3667;

    private sealed record TenantMetadata(int Id, string NexusModsGameDomain, string Name, int NexusModsId);

    private static readonly List<TenantMetadata> TenantMetadatas = new()
    {
        new(BannerlordId, BannerlordGameDomain, BannerlordName, BannerlordNexusModsId),
        new(RimworldId, RimworldGameDomain, RimworldName, RimworldNexusModsId),
        new(StardewValleyId, StardewValleyGameDomain, StardewValleyName, StardewValleyNexusModsId),
        new(ValheimId, ValheimGameDomain, ValheimName, ValheimNexusModsId),
    };

    public static int? FromGameDomainToTenant(string gameDomain) => TenantMetadatas.Find(x => string.Equals(x.NexusModsGameDomain, gameDomain, StringComparison.Ordinal))?.Id;
    public static string? FromTenantToGameDomain(int tenant) => TenantMetadatas.Find(x => x.Id == tenant)?.NexusModsGameDomain;
    public static string? FromTenantToName(int tenant) => TenantMetadatas.Find(x => x.Id == tenant)?.Name;
    public static int? FromTenantToNexusModsId(int tenant) => TenantMetadatas.Find(x => x.Id == tenant)?.NexusModsId;
}