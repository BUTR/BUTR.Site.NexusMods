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
    private NexusModsModEntity(TenantId tenant, NexusModsModId modId) : this() => (TenantId, NexusModsModId) = (tenant, modId);
    [SetsRequiredMembers]
    private NexusModsModEntity(TenantId tenant, NexusModsModId modId, DateTimeOffset lastCheckedDate) : this(tenant, modId) => FileUpdate = new()
    {
        TenantId = tenant,
        NexusModsModId = modId,
        NexusModsMod = this,
        LastCheckedDate = lastCheckedDate
    };

    public static NexusModsModEntity Create(TenantId tenant, NexusModsModId modId) => new(tenant, modId);
    public static NexusModsModEntity Create(TenantId tenant, NexusModsModId modId, DateTimeOffset lastCheckedDate) => new(tenant, modId, lastCheckedDate);
}