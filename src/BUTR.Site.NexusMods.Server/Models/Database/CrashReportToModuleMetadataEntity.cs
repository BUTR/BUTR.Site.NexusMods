using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record CrashReportToModuleMetadataEntity : IEntityWithTenant
{
    public required TenantId TenantId { get; init; }

    public required CrashReportId CrashReportId { get; init; }
    public CrashReportEntity? ToCrashReport { get; init; }

    public required ModuleEntity Module { get; init; }

    public required ModuleVersion Version { get; init; }

    public required NexusModsModEntity? NexusModsMod { get; init; }

    public required bool IsInvolved { get; init; }

    public override int GetHashCode() => HashCode.Combine(CrashReportId, Module.ModuleId, Version, NexusModsMod?.NexusModsModId, IsInvolved);
}