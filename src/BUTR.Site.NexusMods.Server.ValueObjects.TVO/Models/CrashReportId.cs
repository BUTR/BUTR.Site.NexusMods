namespace BUTR.Site.NexusMods.Server.Models;

using TType = CrashReportId;
using TValueType = Guid;

[ValueObject<TValueType>]
public readonly partial struct CrashReportId : IAugmentWith<DefaultValueAugment, JsonAugment, EfCoreAugment>
{
    public static TType DefaultValue => From(Guid.Empty);

    public static TType NewRandomValue(Random? random) => From(Guid.NewGuid());
}

public static class CrashReportIdExtension
{
    public static PropertyBuilder<TType> HasValueObjectConversion(this PropertyBuilder<TType> propertyBuilder) => propertyBuilder
        .HasConversion<TType.EfCoreValueConverter, TType.EfCoreValueComparer>();
}