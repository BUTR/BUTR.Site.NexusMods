using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BUTR.Site.NexusMods.Server.Models.Database;

/// <summary>
/// Building block
/// </summary>
public record ModuleEntity : IEntityWithTenant
{
    public required TenantId TenantId { get; init; }

    public required ModuleId ModuleId { get; init; }
    public ICollection<NexusModsUserToModuleEntity> ToNexusModsUsers { get; init; } = new List<NexusModsUserToModuleEntity>();
    public ICollection<NexusModsModToModuleEntity> ToNexusModsMods { get; init; } = new List<NexusModsModToModuleEntity>();
    public ICollection<StatisticsCrashScoreInvolvedEntity> ToCrashScore { get; init; } = new List<StatisticsCrashScoreInvolvedEntity>();

    public override int GetHashCode() => HashCode.Combine(TenantId, ModuleId);


    private ModuleEntity() { }
    [SetsRequiredMembers]
    private ModuleEntity(TenantId tenant, ModuleId moduleId) : this() => (TenantId, ModuleId) = (tenant, moduleId);

    public static ModuleEntity Create(TenantId tenant, ModuleId moduleId) => new(tenant, moduleId);
}