namespace BUTR.Site.NexusMods.Server.Models;

using TType = ModuleId;
using TValueType = String;

[ValueObject<TValueType>]
public readonly partial struct ModuleId : IAugmentWith<DefaultValueAugment, JsonAugment, EfCoreAugment>
{
    public static TType DefaultValue => From(string.Empty);
}

public static class ModuleIdExtension
{
    public static PropertyBuilder<TType> HasValueObjectConversion(this PropertyBuilder<TType> propertyBuilder) => propertyBuilder
        .HasConversion<TType.EfCoreValueConverter, TType.EfCoreValueComparer>();
}