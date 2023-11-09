using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BUTR.Site.NexusMods.Server.Models.Database;

/// <summary>
/// Building block
/// </summary>
public record NexusModsModEntity : IEntityWithTenant
{
    public required TenantId TenantId { get; init; }

    public required NexusModsModId NexusModsModId { get; init; }
    public NexusModsModToNameEntity? Name { get; init; }
    public NexusModsModToFileUpdateEntity? FileUpdate { get; init; }
    public ICollection<NexusModsModToModuleEntity> ModuleIds { get; init; } = new List<NexusModsModToModuleEntity>();
    public ICollection<NexusModsUserToNexusModsModEntity> ToNexusModsUsers { get; init; } = new List<NexusModsUserToNexusModsModEntity>();

    public override int GetHashCode() => HashCode.Combine(TenantId, NexusModsModId, Name, FileUpdate, ModuleIds);


    private NexusModsModEntity() { }
    [SetsRequiredMembers]
    private NexusModsModEntity(TenantId tenant, NexusModsModId modId) : this() => (TenantId,NexusModsModId) = (tenant, modId);
    [SetsRequiredMembers]
    private NexusModsModEntity(TenantId tenant, NexusModsModId modId, DateTimeOffset lastCheckedDate) : this(tenant, modId) => FileUpdate = new()
    {
        TenantId = tenant,
        NexusModsMod = this,
        LastCheckedDate = lastCheckedDate
    };

    public static NexusModsModEntity Create(TenantId tenant, NexusModsModId modId) => new(tenant, modId);
    public static NexusModsModEntity Create(TenantId tenant, NexusModsModId modId, DateTimeOffset lastCheckedDate) => new(tenant, modId, lastCheckedDate);
}

/*
/// <summary>
/// Building block
/// </summary>
public record SteamUserEntity : IEntity
{
    public required string SteamUserId { get; init; }

    public override int GetHashCode() => HashCode.Combine(SteamUserId);


    private SteamUserEntity() { }
    [SetsRequiredMembers]
    private SteamUserEntity(string steamUserId) : this() => (SteamUserId) = (steamUserId);

    public static SteamUserEntity Create(string steamUserId) => new(steamUserId);
}

/// <summary>
/// Building block
/// </summary>
public record DiscordUserEntity : IEntity
{
    public required string DiscordUserId { get; init; }

    public override int GetHashCode() => HashCode.Combine(DiscordUserId);


    private DiscordUserEntity() { }
    [SetsRequiredMembers]
    private DiscordUserEntity(string discordUserId) : this() => (DiscordUserId) = (discordUserId);

    public static DiscordUserEntity Create(string discordUserId) => new(discordUserId);
}
/// <summary>
/// Building block
/// </summary>
public record GOGUserEntity : IEntity
{
    public required string GOGUserId { get; init; }

    public override int GetHashCode() => HashCode.Combine(GOGUserId);


    private GOGUserEntity() { }
    [SetsRequiredMembers]
    private GOGUserEntity(string gogUserId) : this() => (GOGUserId) = (gogUserId);

    public static GOGUserEntity Create(string gogUserId) => new(gogUserId);
}
*/