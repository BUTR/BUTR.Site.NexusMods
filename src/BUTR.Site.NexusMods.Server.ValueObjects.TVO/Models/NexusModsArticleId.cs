namespace BUTR.Site.NexusMods.Server.Models;

using TType = NexusModsArticleId;
using TValueType = Int32;

[ValueObject<TValueType>]
public readonly partial struct NexusModsArticleId : IAugmentWith<DefaultValueAugment, JsonAugment, EfCoreAugment>
{
    public static readonly TType None = From(0);

    public static TType DefaultValue => None;

    public static bool TryParse(string articleIdRaw, out TType articleId)
    {
        var result = TValueType.TryParse(articleIdRaw, out var articleIdVal);
        articleId = result ? From(articleIdVal) : DefaultValue;
        return result;
    }

    public static bool TryParseUrl(string? urlRaw, out TType articleId)
    {
        articleId = From(0);

        if (!Uri.TryCreate(urlRaw, UriKind.Absolute, out var url))
            return false;

        if (!url.Host.EndsWith("nexusmods.com"))
            return false;

        if (url.LocalPath.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) is not [_, _, var articleIdRaw, ..])
            return false;

        return TryParse(articleIdRaw, out articleId);
    }
}

public static class NexusModsArticleIdExtension
{
    public static PropertyBuilder<TType> HasValueObjectConversion(this PropertyBuilder<TType> propertyBuilder) => propertyBuilder
        .HasConversion<TType.EfCoreValueConverter, TType.EfCoreValueComparer>();
}