using BUTR.CrashReport.Bannerlord.Parser;
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

namespace BUTR.Site.NexusMods.Server.CrashReport.v14;

public static class CrashReportV14
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

    public static bool TryFromHtml(
        ILogger logger,
        IUnitOfWrite unitOfWrite,
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
        CrashReportModel? report;
        try
        {
            report = CrashReportParser.ParseLegacyHtml(version, content);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to parse HTML crash report. FileId: {FileId}, Url: {Url}", fileId, url);
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
        if (version != 14)
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

    private static bool TryFromModel(
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
            OperatingSystemType = report.Metadata.OperatingSystemType.ToString(),
            OperatingSystemVersion = report.Metadata.OperatingSystemVersion,
        };
        crashReportToModuleMetadataEntities = report.Modules.DistinctBy(x => new { x.Id }).Select(x =>
        {
            var nexusModsModId = NexusModsModId.DefaultValue;
            nexusModsModId = NexusModsModId.TryParseUrl(x.Url, out var nexusModsModIdVal) ? nexusModsModIdVal : nexusModsModId;

            var steamWorkshopModId = SteamWorkshopModId.DefaultValue;
            steamWorkshopModId = SteamWorkshopModId.TryParseUrl(x.Url, out var steamWorkshopModIdVal) ? steamWorkshopModIdVal : steamWorkshopModId;
            steamWorkshopModId = x.AdditionalMetadata.FirstOrDefault(x => x.Key == "SteamWorkshopModId") is { Value: { } steamWorkshopModIdStr } && int.TryParse(steamWorkshopModIdStr, out var steamWorkshopModIdRaw) ? SteamWorkshopModId.From(steamWorkshopModIdRaw) : steamWorkshopModId;

            return new CrashReportToModuleMetadataEntity
            {
                TenantId = tenant,
                CrashReportId = crashReportId,
                ModuleId = ModuleId.From(x.Id),
                Module = unitOfWrite.UpsertEntityFactory.GetOrCreateModule(ModuleId.From(x.Id)),
                Version = ModuleVersion.From(x.Version),
                NexusModsModId = nexusModsModId != NexusModsModId.DefaultValue ? nexusModsModId : null,
                NexusModsMod = nexusModsModId != NexusModsModId.DefaultValue ? unitOfWrite.UpsertEntityFactory.GetOrCreateNexusModsMod(nexusModsModId) : null,
                SteamWorkshopModId = steamWorkshopModId != SteamWorkshopModId.DefaultValue ? steamWorkshopModId : null,
                SteamWorkshopMod = steamWorkshopModId != SteamWorkshopModId.DefaultValue ? unitOfWrite.UpsertEntityFactory.GetOrCreateSteamWorkshopMod(steamWorkshopModId) : null,
                InvolvedPosition = (byte) (report.InvolvedModules.IndexOf(y => y.ModuleOrLoaderPluginId == x.Id) + 1),
                IsInvolved = report.InvolvedModules.Any(y => y.ModuleOrLoaderPluginId == x.Id),
            };
        }).ToArray();

        return true;
    }
}