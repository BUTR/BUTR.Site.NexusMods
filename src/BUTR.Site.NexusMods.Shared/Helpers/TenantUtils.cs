using System;
using System.Collections.Generic;

namespace BUTR.Site.NexusMods.Shared.Helpers;

public static class TenantUtils
{
    public const int BannerlordId = 1;
    public const string BannerlordName = "Mount & Blade II: Bannerlord";
    public const string BannerlordGameDomain = "mountandblade2bannerlord";
    public const int BannerlordNexusModsId = 3174;
    public static readonly uint[] BannerlordSteamAppId = [261550];
    public static readonly uint[] BannerlordGOGId = [1802539526, 1564781494];

    public const int RimworldId = 2;
    public const string RimworldName = "Rimworld";
    public const string RimworldGameDomain = "rimworld";
    public const int RimworldNexusModsId = 424;
    public static readonly uint[] RimworldSteamAppId = [294100];
    public static readonly uint[] RimworldGOGId = [1094900565];

    public const int StardewValleyId = 3;
    public const string StardewValleyName = "Stardew Valley";
    public const string StardewValleyGameDomain = "stardewvalley";
    public const int StardewValleyNexusModsId = 1303;
    public static readonly uint[] StardewValleySteamAppIds = [413150];
    public static readonly uint[] StardewValleyGOGIds = [1453375253];

    public const int ValheimId = 4;
    public const string ValheimName = "Valheim";
    public const string ValheimGameDomain = "valheim";
    public const int ValheimNexusModsId = 3667;
    public static readonly uint[] ValheimNexusSteamAppIds = [892970];
    public static readonly uint[] ValheimNexusGOGIds = [];


    public const int TerrariaId = 5;
    public const string TerrariaName = "Terraria";
    public const string TerrariaGameDomain = "terraria";
    public const int TerrariaNexusModsId = 549;
    public static readonly uint[] TerrariaNexusSteamAppIds = [105600];
    public static readonly uint[] TerrariaNexusGOGIds = [1207665503];

    private sealed record TenantMetadata(int Id, string Name, string NexusModsGameDomain, int NexusModsId, uint[] SteamAppIds, uint[] GOGIds);

    private static readonly List<TenantMetadata> TenantMetadatas = new()
    {
        new(BannerlordId, BannerlordName, BannerlordGameDomain, BannerlordNexusModsId, BannerlordSteamAppId, BannerlordGOGId),
        new(RimworldId, RimworldName, RimworldGameDomain, RimworldNexusModsId, RimworldSteamAppId, RimworldGOGId),
        new(StardewValleyId, StardewValleyName, StardewValleyGameDomain, StardewValleyNexusModsId, StardewValleySteamAppIds, StardewValleyGOGIds),
        new(ValheimId, ValheimName, ValheimGameDomain, ValheimNexusModsId, ValheimNexusSteamAppIds, ValheimNexusGOGIds),
        new(TerrariaId, TerrariaName, TerrariaGameDomain, TerrariaNexusModsId, TerrariaNexusSteamAppIds, TerrariaNexusGOGIds),
    };

    public static int? FromGameDomainToTenant(string gameDomain) => TenantMetadatas.Find(x => string.Equals(x.NexusModsGameDomain, gameDomain, StringComparison.Ordinal))?.Id;
    public static string? FromTenantToGameDomain(int tenant) => TenantMetadatas.Find(x => x.Id == tenant)?.NexusModsGameDomain;
    public static string? FromTenantToName(int tenant) => TenantMetadatas.Find(x => x.Id == tenant)?.Name;
    public static int? FromTenantToNexusModsId(int tenant) => TenantMetadatas.Find(x => x.Id == tenant)?.NexusModsId;
    public static uint[] FromTenantToSteamAppIds(uint tenant) => TenantMetadatas.Find(x => x.Id == tenant)?.SteamAppIds ?? [];
    public static uint[] FromTenantToGOGIds(uint tenant) => TenantMetadatas.Find(x => x.Id == tenant)?.GOGIds ?? [];
}