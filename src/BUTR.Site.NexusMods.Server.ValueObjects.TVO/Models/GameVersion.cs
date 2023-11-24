namespace BUTR.Site.NexusMods.Server.Models;

using TType = GameVersion;
using TValueType = String;

[ValueObject<TValueType>]
public readonly partial struct GameVersion : IAugmentWith<DefaultValueAugment, JsonAugment, EfCoreAugment>
{
    public static TType DefaultValue => From(string.Empty);
}

public static class GameVersionExtension
{
    public static PropertyBuilder<TType> HasValueObjectConversion(this PropertyBuilder<TType> propertyBuilder) => propertyBuilder
        .HasConversion<TType.EfCoreValueConverter, TType.EfCoreValueComparer>();
}