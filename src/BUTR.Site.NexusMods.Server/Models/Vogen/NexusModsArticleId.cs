using BUTR.Site.NexusMods.Server.Utils.Vogen;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

using Vogen;

namespace BUTR.Site.NexusMods.Server.Models;

using TType = NexusModsArticleId;
using TValueType = Int32;

[TypeConverter(typeof(VogenTypeConverter<TType, TValueType>))]
[JsonConverter(typeof(VogenJsonConverter<TType, TValueType>))]
[ValueObject<TValueType>(conversions: Conversions.None)]
[Instance("None", 0)]
public readonly partial record struct NexusModsArticleId : IVogen<TType, TValueType>, IVogenParsable<TType, TValueType>, IVogenSpanParsable<TType, TValueType>, IVogenUtf8SpanParsable<TType, TValueType>, IHasDefaultValue<TType>
{
    public static TType Copy(TType instance) => instance with { };
    public static bool IsInitialized(TType instance) => instance._isInitialized;
    public static TType DeserializeDangerous(TValueType instance) => Deserialize(instance);
    public static NexusModsArticleId DefaultValue => None;

    public static bool TryParseUrl(string urlRaw, out TType articleId)
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

    public static int GetHashCode(TType instance) => VogenDefaults<TType, TValueType>.GetHashCode(instance);

    public static bool Equals(TType left, TType right) => VogenDefaults<TType, TValueType>.Equals(left, right);
    public static bool Equals(TType left, TType right, IEqualityComparer<TType> comparer) => VogenDefaults<TType, TValueType>.Equals(left, right, comparer);

    public static int CompareTo(TType left, TType right) => VogenDefaults<TType, TValueType>.CompareTo(left, right);
}