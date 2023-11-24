namespace BUTR.Site.NexusMods.Server.Models;

using TType = NexusModsModId;
using TValueType = Int32;

[ValueObject<TValueType>]
public readonly partial struct NexusModsModId : IAugmentWith<DefaultValueAugment, JsonAugment, EfCoreAugment>
{
    public static readonly TType None = From(0);
    public static TType DefaultValue => None;

    public static bool TryParse(string modIdRaw, out TType modId)
    {
        var result = TValueType.TryParse(modIdRaw, out var modIdVal);
        modId = result ? From(modIdVal) : DefaultValue;
        return result;
    }
    
    public static bool TryParseUrl(string? urlRaw, out TType modId)
    {
        modId = DefaultValue;

        if (!Uri.TryCreate(urlRaw, UriKind.Absolute, out var url))
            return false;

        if (!url.Host.EndsWith("nexusmods.com"))
            return false;

        if (url.LocalPath.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) is not [_, _, var modIdRaw, ..])
            return false;

        return TryParse(modIdRaw, out modId);
    }
}

public static class NexusModsModIdExtension
{
    public static PropertyBuilder<TType> HasValueObjectConversion(this PropertyBuilder<TType> propertyBuilder) => propertyBuilder
        .HasConversion<TType.EfCoreValueConverter, TType.EfCoreValueComparer>();

    public static PropertyBuilder<TType?> HasValueObjectConversion(this PropertyBuilder<TType?> propertyBuilder) => propertyBuilder
        .HasConversion<TType.EfCoreValueConverter, TType.EfCoreValueComparer>();
}