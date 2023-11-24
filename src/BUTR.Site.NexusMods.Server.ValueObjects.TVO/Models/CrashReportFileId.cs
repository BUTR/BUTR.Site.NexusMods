namespace BUTR.Site.NexusMods.Server.Models;

using TType = CrashReportFileId;
using TValueType = String;

[ValueObject<TValueType>]
public readonly partial struct CrashReportFileId : IAugmentWith<DefaultValueAugment, JsonAugment, EfCoreAugment>
{
    public static TType DefaultValue => From(string.Empty);
}

public static class CrashReportFileIdExtension
{
    public static PropertyBuilder<TType> HasValueObjectConversion(this PropertyBuilder<TType> propertyBuilder) => propertyBuilder
        .HasConversion<TType.EfCoreValueConverter, TType.EfCoreValueComparer>();
}