using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BUTR.Site.NexusMods.Server.Models.Database;

/// <summary>
/// Building block
/// </summary>
public record SteamWorkshopModEntity : IEntityWithTenant
{
    public required TenantId TenantId { get; init; }

    public required SteamWorkshopModId SteamWorkshopModId { get; init; }
    public SteamWorkshopModToNameEntity? Name { get; init; }
    public SteamWorkshopModToFileUpdateEntity? FileUpdate { get; init; }
    public ICollection<SteamWorkshopModToModuleEntity> ModuleIds { get; init; } = new List<SteamWorkshopModToModuleEntity>();
    public ICollection<NexusModsUserToSteamWorkshopModEntity> ToNexusModsUsers { get; init; } = new List<NexusModsUserToSteamWorkshopModEntity>();

    public override int GetHashCode() => HashCode.Combine(TenantId, SteamWorkshopModId, Name, FileUpdate, ModuleIds);


    private SteamWorkshopModEntity() { }
    [SetsRequiredMembers]
    private SteamWorkshopModEntity(TenantId tenant, SteamWorkshopModId modId) : this() => (TenantId, SteamWorkshopModId) = (tenant, modId);
    [SetsRequiredMembers]
    private SteamWorkshopModEntity(TenantId tenant, SteamWorkshopModId modId, DateTimeOffset lastCheckedDate) : this(tenant, modId) => FileUpdate = new()
    {
        TenantId = tenant,
        SteamWorkshopModId = modId,
        SteamWorkshopMod = this,
        LastCheckedDate = lastCheckedDate
    };

    public static SteamWorkshopModEntity Create(TenantId tenant, SteamWorkshopModId modId) => new(tenant, modId);
    public static SteamWorkshopModEntity Create(TenantId tenant, SteamWorkshopModId modId, DateTimeOffset lastCheckedDate) => new(tenant, modId, lastCheckedDate);
}