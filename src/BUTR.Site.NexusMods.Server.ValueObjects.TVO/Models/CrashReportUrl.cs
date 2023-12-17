namespace BUTR.Site.NexusMods.Server.Models;

using TType = CrashReportUrl;
using TValueType = String;

[ValueObject<TValueType>]
public readonly partial struct CrashReportUrl : IAugmentWith<DefaultValueAugment, JsonAugment, EfCoreAugment>
{
    public static TType DefaultValue => From(string.Empty);

    public static TType From(Uri uri) => From(uri.ToString());
}

public static class CrashReportUrlExtension
{
    public static PropertyBuilder<TType> HasValueObjectConversion(this PropertyBuilder<TType> propertyBuilder) => propertyBuilder
        .HasConversion<TType.EfCoreValueConverter, TType.EfCoreValueComparer>();
}