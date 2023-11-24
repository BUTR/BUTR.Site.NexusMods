namespace BUTR.Site.NexusMods.Server.Models;

public static class EfCoreExtensions
{
    private static PropertyBuilder<TVogen> HasValueObjectConversion<TVogen, TValueObject>(this PropertyBuilder<TVogen> propertyBuilder)
        where TVogen : struct, IVogen<TVogen, TValueObject>, IEquatable<TVogen>, IEquatable<TValueObject>, IComparable<TVogen>, IComparable
        where TValueObject : notnull =>
        propertyBuilder.HasConversion<VogenValueConverter<TVogen, TValueObject>, VogenValueComparer<TVogen, TValueObject>>();

    private static PropertyBuilder<TNullable> HasVogenConversionNullable<TNullable, TVogen, TValueObject>(this PropertyBuilder<TNullable> propertyBuilder)
        where TVogen : struct, IVogen<TVogen, TValueObject>, IEquatable<TVogen>, IEquatable<TValueObject>, IComparable<TVogen>, IComparable
        where TValueObject : notnull =>
        propertyBuilder.HasConversion<VogenValueConverter<TVogen, TValueObject>, VogenValueComparer<TVogen, TValueObject>>();


    public static PropertyBuilder<ApplicationRole> HasValueObjectConversion(this PropertyBuilder<ApplicationRole> propertyBuilder) =>
        propertyBuilder.HasValueObjectConversion<ApplicationRole, string>();

    public static PropertyBuilder<CrashReportFileId> HasValueObjectConversion(this PropertyBuilder<CrashReportFileId> propertyBuilder) =>
        propertyBuilder.HasValueObjectConversion<CrashReportFileId, string>();

    public static PropertyBuilder<CrashReportId> HasValueObjectConversion(this PropertyBuilder<CrashReportId> propertyBuilder) =>
        propertyBuilder.HasValueObjectConversion<CrashReportId, Guid>();

    public static PropertyBuilder<CrashReportUrl> HasValueObjectConversion(this PropertyBuilder<CrashReportUrl> propertyBuilder) =>
        propertyBuilder.HasValueObjectConversion<CrashReportUrl, string>();

    public static PropertyBuilder<CrashReportVersion> HasValueObjectConversion(this PropertyBuilder<CrashReportVersion> propertyBuilder) =>
        propertyBuilder.HasValueObjectConversion<CrashReportVersion, byte>();

    public static PropertyBuilder<ExceptionTypeId> HasValueObjectConversion(this PropertyBuilder<ExceptionTypeId> propertyBuilder) =>
        propertyBuilder.HasValueObjectConversion<ExceptionTypeId, string>();

    public static PropertyBuilder<GameVersion> HasValueObjectConversion(this PropertyBuilder<GameVersion> propertyBuilder) =>
        propertyBuilder.HasValueObjectConversion<GameVersion, string>();

    public static PropertyBuilder<ModuleId> HasValueObjectConversion(this PropertyBuilder<ModuleId> propertyBuilder) =>
        propertyBuilder.HasValueObjectConversion<ModuleId, string>();

    public static PropertyBuilder<ModuleVersion> HasValueObjectConversion(this PropertyBuilder<ModuleVersion> propertyBuilder) =>
        propertyBuilder.HasValueObjectConversion<ModuleVersion, string>();

    public static PropertyBuilder<NexusModsApiKey> HasValueObjectConversion(this PropertyBuilder<NexusModsApiKey> propertyBuilder) =>
        propertyBuilder.HasValueObjectConversion<NexusModsApiKey, string>();

    public static PropertyBuilder<NexusModsArticleId> HasValueObjectConversion(this PropertyBuilder<NexusModsArticleId> propertyBuilder) =>
        propertyBuilder.HasValueObjectConversion<NexusModsArticleId, int>();

    public static PropertyBuilder<NexusModsFileId> HasValueObjectConversion(this PropertyBuilder<NexusModsFileId> propertyBuilder) =>
        propertyBuilder.HasValueObjectConversion<NexusModsFileId, int>();

    public static PropertyBuilder<NexusModsGameDomain> HasValueObjectConversion(this PropertyBuilder<NexusModsGameDomain> propertyBuilder) =>
        propertyBuilder.HasValueObjectConversion<NexusModsGameDomain, string>();

    public static PropertyBuilder<NexusModsModId> HasValueObjectConversion(this PropertyBuilder<NexusModsModId> propertyBuilder) =>
        propertyBuilder.HasValueObjectConversion<NexusModsModId, int>();

    public static PropertyBuilder<NexusModsUserEMail> HasValueObjectConversion(this PropertyBuilder<NexusModsUserEMail> propertyBuilder) =>
        propertyBuilder.HasValueObjectConversion<NexusModsUserEMail, string>();

    public static PropertyBuilder<NexusModsUserId> HasValueObjectConversion(this PropertyBuilder<NexusModsUserId> propertyBuilder) =>
        propertyBuilder.HasValueObjectConversion<NexusModsUserId, int>();

    public static PropertyBuilder<NexusModsUserName> HasValueObjectConversion(this PropertyBuilder<NexusModsUserName> propertyBuilder) =>
        propertyBuilder.HasValueObjectConversion<NexusModsUserName, string>();

    public static PropertyBuilder<TenantId> HasValueObjectConversion(this PropertyBuilder<TenantId> propertyBuilder) =>
        propertyBuilder.HasValueObjectConversion<TenantId, byte>();


    public static PropertyBuilder<NexusModsModId?> HasValueObjectConversion(this PropertyBuilder<NexusModsModId?> propertyBuilder) =>
        propertyBuilder.HasVogenConversionNullable<NexusModsModId?, NexusModsModId, int>();
}