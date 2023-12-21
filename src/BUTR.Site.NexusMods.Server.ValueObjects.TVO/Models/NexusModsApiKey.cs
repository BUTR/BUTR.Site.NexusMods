namespace BUTR.Site.NexusMods.Server.Models;

using TType = NexusModsApiKey;
using TValueType = String;

[TypeConverter(type: typeof(TransparentValueObjectTypeConverter<TType, TValueType>))]
[ValueObject<TValueType>]
public readonly partial struct NexusModsApiKey : IAugmentWith<DefaultValueAugment, JsonAugment, EfCoreAugment>, IValueObjectFrom<TType, TValueType>
{
    public static readonly TType None = From(string.Empty);

    public static TType DefaultValue => None;
}

public static class NexusModsApiKeyExtension
{
    public static PropertyBuilder<TType> HasValueObjectConversion(this PropertyBuilder<TType> propertyBuilder) => propertyBuilder
        .HasConversion<TType.EfCoreValueConverter, TType.EfCoreValueComparer>();
}