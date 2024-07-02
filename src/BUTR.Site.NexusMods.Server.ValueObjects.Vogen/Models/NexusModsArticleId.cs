namespace BUTR.Site.NexusMods.Server.Models;

using TType = NexusModsArticleId;
using TValueType = Int32;

[ValueObject<TValueType>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson | Conversions.TypeConverter)]
public readonly partial struct NexusModsArticleId : IHasDefaultValue<TType>
{
    public static readonly TType None = new(0);

    public static TType DefaultValue => None;

    public static bool TryParseUrl(string urlRaw, out TType articleId)
    {
        articleId = From(0);

        if (!Uri.TryCreate(urlRaw, UriKind.Absolute, out var url))
            return false;

        if (!url.Host.EndsWith("nexusmods.com"))
            return false;

        if (url.LocalPath.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) is not [_, var articles, var articleIdRaw, ..])
            return false;

        if (!string.Equals(articles, "articles", StringComparison.OrdinalIgnoreCase))
            return false;

        return TryParse(articleIdRaw, out articleId);
    }
}