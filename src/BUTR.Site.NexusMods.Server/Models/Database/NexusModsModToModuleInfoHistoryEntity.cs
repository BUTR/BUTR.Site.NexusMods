using System;
using System.Collections.Generic;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record NexusModsModToModuleInfoHistoryEntity : IEntityWithTenant
{
    public required TenantId TenantId { get; init; }
    public required NexusModsModId NexusModsModId { get; init; }
    public required NexusModsModEntity NexusModsMod { get; init; }
    public required ModuleId ModuleId { get; init; }
    public required ModuleEntity Module { get; init; }
    public required ModuleVersion ModuleVersion { get; init; }
    public required NexusModsFileId NexusModsFileId { get; init; }
    public required DateTimeOffset UploadDate { get; init; }

    public required ModuleInfoModel ModuleInfo { get; init; }

    public ICollection<NexusModsModToModuleInfoHistoryGameVersionEntity> GameVersions { get; init; } = new List<NexusModsModToModuleInfoHistoryGameVersionEntity>();


    public override int GetHashCode() => HashCode.Combine(TenantId, NexusModsModId, ModuleId, ModuleVersion, NexusModsFileId, UploadDate);
}