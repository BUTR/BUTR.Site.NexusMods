using BUTR.CrashReport.Models;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Repositories;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;

namespace BUTR.Site.NexusMods.Server.CrashReport.v13;

public static class CrashReportV13
{
    private static ExceptionTypeId FromException(ExceptionModel exception)
    {
        var exc = exception;
        while (exc.InnerException is not null)
            exc = exc.InnerException;

        return ExceptionTypeId.From(exc.Type);
    }

    private static string GetException(ExceptionModel? exception, bool inner = false) => exception is null ? string.Empty : $"""

{(inner ? "Inner " : string.Empty)}Exception information
Type: {exception.Type}
Message: {exception.Message}
CallStack:
{exception.CallStack}

{GetException(exception.InnerException, true)}
""";

    public static bool TryFromJson(
        ILogger logger,
        IUnitOfWrite unitOfWrite,
        JsonSerializerOptions jsonSerializerOptions,
        TenantId tenant,
        CrashReportFileId fileId,
        CrashReportUrl url,
        DateTime date,
        byte version,
        string content,
        [NotNullWhen(true)] out CrashReportEntity? crashReportEntity,
        [NotNullWhen(true)] out CrashReportToMetadataEntity? crashReportToMetadataEntity,
        [NotNullWhen(true)] out IList<CrashReportToModuleMetadataEntity>? crashReportToModuleMetadataEntities)
    {
        if (version != 13)
        {
            crashReportEntity = null!;
            crashReportToMetadataEntity = null!;
            crashReportToModuleMetadataEntities = null!;
            return false;
        }

        CrashReportModel? report;
        try
        {
            report = JsonSerializer.Deserialize<CrashReportModel>(content, jsonSerializerOptions);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to parse JSON crash report. FileId: {FileId}, Url: {Url}", fileId, url);
            report = null;
        }

        if (report is null)
        {
            crashReportEntity = null!;
            crashReportToMetadataEntity = null!;
            crashReportToModuleMetadataEntities = null!;
            return false;
        }

        return TryFromModel(unitOfWrite, tenant, fileId, url, date, report, out crashReportEntity, out crashReportToMetadataEntity, out crashReportToModuleMetadataEntities);
    }

    public static bool TryFromModel(
        IUnitOfWrite unitOfWrite,
        TenantId tenant,
        CrashReportFileId fileId,
        CrashReportUrl url,
        DateTime date,
        CrashReportModel report,
        [NotNullWhen(true)] out CrashReportEntity? crashReportEntity,
        [NotNullWhen(true)] out CrashReportToMetadataEntity? crashReportToMetadataEntity,
        [NotNullWhen(true)] out IList<CrashReportToModuleMetadataEntity>? crashReportToModuleMetadataEntities)
    {
        var butrLoaderVersion = report.Metadata.AdditionalMetadata.FirstOrDefault(x => x.Key == "BUTRLoaderVersion")?.Value;
        var blseVersion = report.Metadata.AdditionalMetadata.FirstOrDefault(x => x.Key == "BLSEVersion")?.Value;
        var launcherExVersion = report.Metadata.AdditionalMetadata.FirstOrDefault(x => x.Key == "LauncherExVersion")?.Value;

        var crashReportId = CrashReportId.From(report.Id);
        crashReportEntity = new CrashReportEntity
        {
            TenantId = tenant,
            CrashReportId = crashReportId,
            Url = url,
            Version = CrashReportVersion.From(report.Version),
            GameVersion = GameVersion.From(report.Metadata.GameVersion),
            ExceptionTypeId = FromException(report.Exception),
            ExceptionType = unitOfWrite.UpsertEntityFactory.GetOrCreateExceptionType(FromException(report.Exception)),
            Exception = GetException(report.Exception),
            CreatedAt = fileId.Value.Length == 8 ? DateTimeOffset.UnixEpoch.ToUniversalTime() : date.ToUniversalTime(),
        };
        crashReportToMetadataEntity = new CrashReportToMetadataEntity
        {
            TenantId = tenant,
            CrashReportId = crashReportId,
            LauncherType = string.IsNullOrEmpty(report.Metadata.LauncherType) ? null : report.Metadata.LauncherType,
            LauncherVersion = string.IsNullOrEmpty(report.Metadata.LauncherVersion) ? null : report.Metadata.LauncherVersion,
            Runtime = string.IsNullOrEmpty(report.Metadata.Runtime) ? null : report.Metadata.Runtime,
            BUTRLoaderVersion = butrLoaderVersion,
            BLSEVersion = blseVersion,
            LauncherExVersion = launcherExVersion,
            OperatingSystemType = null,
            OperatingSystemVersion = null,
        };
        crashReportToModuleMetadataEntities = report.Modules.DistinctBy(x => new { x.Id }).Select(x => new CrashReportToModuleMetadataEntity
        {
            TenantId = tenant,
            CrashReportId = crashReportId,
            ModuleId = ModuleId.From(x.Id),
            Module = unitOfWrite.UpsertEntityFactory.GetOrCreateModule(ModuleId.From(x.Id)),
            Version = ModuleVersion.From(x.Version),
            NexusModsModId = NexusModsModId.TryParseUrl(x.Url, out var modId1) ? modId1 : null,
            NexusModsMod = NexusModsModId.TryParseUrl(x.Url, out var modId2) ? unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsMod(modId2) : null,
            InvolvedPosition = (byte) (report.InvolvedModules.IndexOf(y => y.ModuleOrLoaderPluginId == x.Id) + 1),
            IsInvolved = report.InvolvedModules.Any(y => y.ModuleOrLoaderPluginId == x.Id),
        }).ToArray();

        return true;
    }
}