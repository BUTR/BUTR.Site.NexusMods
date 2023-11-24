namespace BUTR.Site.NexusMods.Server.Models;

using TType = ModuleVersion;
using TValueType = String;

[ValueObject<TValueType>]
public readonly partial struct ModuleVersion : IAugmentWith<DefaultValueAugment, JsonAugment, EfCoreAugment>
{
    public static TType DefaultValue => From(string.Empty);
}

public static class ModuleVersionExtension
{
    public static PropertyBuilder<TType> HasValueObjectConversion(this PropertyBuilder<TType> propertyBuilder) => propertyBuilder
        .HasConversion<TType.EfCoreValueConverter, TType.EfCoreValueComparer>();
}