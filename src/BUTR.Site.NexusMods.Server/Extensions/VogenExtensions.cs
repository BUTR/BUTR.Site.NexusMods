using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Utils.Vogen;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

using System;

namespace BUTR.Site.NexusMods.Server.Extensions;

public static class VogenExtensions
{
    private static PropertyBuilder<TVogen> HasVogenConversion<TVogen, TValueObject>(this PropertyBuilder<TVogen> propertyBuilder)
        where TVogen : struct, IVogen<TVogen, TValueObject>, IEquatable<TVogen>, IEquatable<TValueObject>, IComparable<TVogen>, IComparable
        where TValueObject : notnull =>
        propertyBuilder.HasConversion<VogenValueConverter<TVogen, TValueObject>, VogenValueComparer<TVogen, TValueObject>>();

    private static PropertyBuilder<TNullable> HasVogenConversionNullable<TNullable, TVogen, TValueObject>(this PropertyBuilder<TNullable> propertyBuilder)
        where TVogen : struct, IVogen<TVogen, TValueObject>, IEquatable<TVogen>, IEquatable<TValueObject>, IComparable<TVogen>, IComparable
        where TValueObject : notnull =>
        propertyBuilder.HasConversion<VogenValueConverter<TVogen, TValueObject>, VogenValueComparer<TVogen, TValueObject>>();


    public static PropertyBuilder<ApplicationRole> HasVogenConversion(this PropertyBuilder<ApplicationRole> propertyBuilder) =>
        propertyBuilder.HasVogenConversion<ApplicationRole, string>();

    public static PropertyBuilder<CrashReportFileId> HasVogenConversion(this PropertyBuilder<CrashReportFileId> propertyBuilder) =>
        propertyBuilder.HasVogenConversion<CrashReportFileId, string>();

    public static PropertyBuilder<CrashReportId> HasVogenConversion(this PropertyBuilder<CrashReportId> propertyBuilder) =>
        propertyBuilder.HasVogenConversion<CrashReportId, Guid>();

    public static PropertyBuilder<CrashReportUrl> HasVogenConversion(this PropertyBuilder<CrashReportUrl> propertyBuilder) =>
        propertyBuilder.HasVogenConversion<CrashReportUrl, string>();

    public static PropertyBuilder<CrashReportVersion> HasVogenConversion(this PropertyBuilder<CrashReportVersion> propertyBuilder) =>
        propertyBuilder.HasVogenConversion<CrashReportVersion, byte>();

    public static PropertyBuilder<ExceptionTypeId> HasVogenConversion(this PropertyBuilder<ExceptionTypeId> propertyBuilder) =>
        propertyBuilder.HasVogenConversion<ExceptionTypeId, string>();

    public static PropertyBuilder<GameVersion> HasVogenConversion(this PropertyBuilder<GameVersion> propertyBuilder) =>
        propertyBuilder.HasVogenConversion<GameVersion, string>();

    public static PropertyBuilder<ModuleId> HasVogenConversion(this PropertyBuilder<ModuleId> propertyBuilder) =>
        propertyBuilder.HasVogenConversion<ModuleId, string>();

    public static PropertyBuilder<ModuleVersion> HasVogenConversion(this PropertyBuilder<ModuleVersion> propertyBuilder) =>
        propertyBuilder.HasVogenConversion<ModuleVersion, string>();

    public static PropertyBuilder<NexusModsApiKey> HasVogenConversion(this PropertyBuilder<NexusModsApiKey> propertyBuilder) =>
        propertyBuilder.HasVogenConversion<NexusModsApiKey, string>();

    public static PropertyBuilder<NexusModsArticleId> HasVogenConversion(this PropertyBuilder<NexusModsArticleId> propertyBuilder) =>
        propertyBuilder.HasVogenConversion<NexusModsArticleId, int>();

    public static PropertyBuilder<NexusModsFileId> HasVogenConversion(this PropertyBuilder<NexusModsFileId> propertyBuilder) =>
        propertyBuilder.HasVogenConversion<NexusModsFileId, int>();

    public static PropertyBuilder<NexusModsGameDomain> HasVogenConversion(this PropertyBuilder<NexusModsGameDomain> propertyBuilder) =>
        propertyBuilder.HasVogenConversion<NexusModsGameDomain, string>();

    public static PropertyBuilder<NexusModsModId> HasVogenConversion(this PropertyBuilder<NexusModsModId> propertyBuilder) =>
        propertyBuilder.HasVogenConversion<NexusModsModId, int>();

    public static PropertyBuilder<NexusModsUserEMail> HasVogenConversion(this PropertyBuilder<NexusModsUserEMail> propertyBuilder) =>
        propertyBuilder.HasVogenConversion<NexusModsUserEMail, string>();

    public static PropertyBuilder<NexusModsUserId> HasVogenConversion(this PropertyBuilder<NexusModsUserId> propertyBuilder) =>
        propertyBuilder.HasVogenConversion<NexusModsUserId, int>();

    public static PropertyBuilder<NexusModsUserName> HasVogenConversion(this PropertyBuilder<NexusModsUserName> propertyBuilder) =>
        propertyBuilder.HasVogenConversion<NexusModsUserName, string>();

    public static PropertyBuilder<TenantId> HasVogenConversion(this PropertyBuilder<TenantId> propertyBuilder) =>
        propertyBuilder.HasVogenConversion<TenantId, byte>();


    public static PropertyBuilder<NexusModsModId?> HasVogenConversion(this PropertyBuilder<NexusModsModId?> propertyBuilder) =>
        propertyBuilder.HasVogenConversionNullable<NexusModsModId?, NexusModsModId, int>();
}