namespace BUTR.Site.NexusMods.Server.Models;

using TType = NexusModsFileId;
using TValueType = Int32;

[ValueObject<TValueType>]
public readonly partial struct NexusModsFileId : IAugmentWith<DefaultValueAugment, JsonAugment, EfCoreAugment>
{
    public static TType DefaultValue => From(0);
}

public static class NexusModsFileIdExtension
{
    public static PropertyBuilder<TType> HasValueObjectConversion(this PropertyBuilder<TType> propertyBuilder) => propertyBuilder
        .HasConversion<TType.EfCoreValueConverter, TType.EfCoreValueComparer>();
}