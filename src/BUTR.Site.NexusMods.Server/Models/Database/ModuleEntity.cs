using BUTR.Site.NexusMods.Shared;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BUTR.Site.NexusMods.Server.Models.Database;

/// <summary>
/// Building block
/// </summary>
public record ModuleEntity : IEntityWithTenant
{
    public required Tenant TenantId { get; init; }

    public required string ModuleId { get; init; }
    public ICollection<NexusModsUserToModuleEntity> ToNexusModsUsers { get; init; } = new List<NexusModsUserToModuleEntity>();
    public ICollection<NexusModsModToModuleEntity> ToNexusModsMods { get; init; } = new List<NexusModsModToModuleEntity>();
    public ICollection<StatisticsCrashScoreInvolvedEntity> ToCrashScore { get; init; } = new List<StatisticsCrashScoreInvolvedEntity>();

    public override int GetHashCode() => HashCode.Combine(TenantId, ModuleId);


    private ModuleEntity() { }
    [SetsRequiredMembers]
    private ModuleEntity(Tenant tenant, string moduleId) : this() => (TenantId, ModuleId) = (tenant, moduleId);

    public static ModuleEntity Create(Tenant tenant, string moduleId) => new(tenant, moduleId);
}