namespace BUTR.Site.NexusMods.Server.Models;

using TType = CrashReportVersion;
using TValueType = Byte;

[ValueObject<TValueType>]
public readonly partial struct CrashReportVersion : IAugmentWith<DefaultValueAugment, JsonAugment, EfCoreAugment>
{
    public static TType DefaultValue => From(0);
}

public static class CrashReportVersionExtension
{
    public static PropertyBuilder<TType> HasValueObjectConversion(this PropertyBuilder<TType> propertyBuilder) => propertyBuilder
        .HasConversion<TType.EfCoreValueConverter, TType.EfCoreValueComparer>();
}