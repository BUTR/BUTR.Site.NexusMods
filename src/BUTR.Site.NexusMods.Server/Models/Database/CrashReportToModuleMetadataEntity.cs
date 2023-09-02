using BUTR.Site.NexusMods.Shared;

using System;

namespace BUTR.Site.NexusMods.Server.Models.Database;

public sealed record CrashReportToModuleMetadataEntity : IEntityWithTenant
{
    public required Tenant TenantId { get; init; }

    public required Guid CrashReportId { get; init; }
    public CrashReportEntity? ToCrashReport { get; init; }

    public required ModuleEntity Module { get; init; }

    public required string Version { get; init; }

    public required NexusModsModEntity? NexusModsMod { get; init; }

    public required bool IsInvolved { get; init; }

    public override int GetHashCode() => HashCode.Combine(CrashReportId, Module.ModuleId, Version, NexusModsMod?.NexusModsModId, IsInvolved);
}