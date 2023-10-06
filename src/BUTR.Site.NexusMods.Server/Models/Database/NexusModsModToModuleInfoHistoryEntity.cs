using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record NexusModsModToModuleInfoHistoryEntity : IEntityWithTenant
{
    public required TenantId TenantId { get; init; }
    public required NexusModsModEntity NexusModsMod { get; init; }
    public required ModuleEntity Module { get; init; }
    public required ModuleVersion ModuleVersion { get; init; }

    public required ModuleInfoModel ModuleInfo { get; init; }


    public override int GetHashCode() => HashCode.Combine(TenantId, NexusModsMod.NexusModsModId, Module.ModuleId, ModuleVersion);
}