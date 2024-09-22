using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record NexusModsModToModuleInfoHistoryGameVersionEntity : IEntityWithTenant
{
    public required TenantId TenantId { get; init; }
    public required NexusModsModId NexusModsModId { get; init; }
    public required NexusModsModEntity NexusModsMod { get; init; }
    public required ModuleId ModuleId { get; init; }
    public required ModuleEntity Module { get; init; }
    public required ModuleVersion ModuleVersion { get; init; }
    public required NexusModsFileId NexusModsFileId { get; init; }

    public required GameVersion GameVersion { get; init; }

    public NexusModsModToModuleInfoHistoryEntity MainEntity { get; init; } = default!;

    public override int GetHashCode() => HashCode.Combine(TenantId, NexusModsModId, ModuleId, ModuleVersion, NexusModsFileId, GameVersion);

}