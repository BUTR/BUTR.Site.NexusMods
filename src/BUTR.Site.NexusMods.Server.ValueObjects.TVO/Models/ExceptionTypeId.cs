namespace BUTR.Site.NexusMods.Server.Models;

using TType = ExceptionTypeId;
using TValueType = String;

[ValueObject<TValueType>]
public readonly partial struct ExceptionTypeId : IAugmentWith<DefaultValueAugment, JsonAugment, EfCoreAugment>
{
    public static TType DefaultValue => From(string.Empty);
}

public static class ExceptionTypeIdExtension
{
    public static PropertyBuilder<TType> HasValueObjectConversion(this PropertyBuilder<TType> propertyBuilder) => propertyBuilder
        .HasConversion<TType.EfCoreValueConverter, TType.EfCoreValueComparer>();
}