namespace BUTR.Site.NexusMods.Server.Models;

using TType = NexusModsUserName;
using TValueType = String;

[ValueObject<TValueType>]
public readonly partial struct NexusModsUserName : IAugmentWith<DefaultValueAugment, JsonAugment, EfCoreAugment>
{
    public static readonly TType Empty = From(string.Empty);

    public static TType DefaultValue => Empty;
}

public static class NexusModsUserNameExtension
{
    public static PropertyBuilder<TType> HasValueObjectConversion(this PropertyBuilder<TType> propertyBuilder) => propertyBuilder
        .HasConversion<TType.EfCoreValueConverter, TType.EfCoreValueComparer>();
}