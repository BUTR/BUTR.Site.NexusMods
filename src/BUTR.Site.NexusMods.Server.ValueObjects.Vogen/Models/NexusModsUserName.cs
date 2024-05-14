namespace BUTR.Site.NexusMods.Server.Models;

using TType = NexusModsUserName;
using TValueType = String;

[ValueObject<TValueType>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson | Conversions.TypeConverter)]
public readonly partial struct NexusModsUserName : IVogen<TType, TValueType>, IHasDefaultValue<TType>
{
    public static readonly TType Empty = From(string.Empty);

    public static TType DefaultValue => Empty;

    public static TType Copy(TType instance) => instance with { };


    public static bool TryParseUrl(string urlRaw, out TType username)
    {
        username = DefaultValue;

        if (!Uri.TryCreate(urlRaw, UriKind.Absolute, out var url))
            return false;

        if (!urlRaw.Contains("nexusmods.com/profile/", StringComparison.OrdinalIgnoreCase))
            return false;

        if (url.LocalPath.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) is not [_, var usernameRaw, ..])
            return false;

        username = From(usernameRaw);
        return true;

        /*
        username = default;

        if (url is null)
            return false;

        if (!url.Contains("nexusmods.com/profile/", StringComparison.OrdinalIgnoreCase))
            return false;

        var str1 = url.ToLowerInvariant().Split("nexusmods.com/profile/");
        if (str1.Length != 2)
            return false;

        var split = str1[1].Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (split.Length < 2)
            return false;

        username = split[2];
        return true;
        */
    }

    public static int GetHashCode(TType instance) => VogenDefaults<TType, TValueType>.GetHashCode(instance);

    public static bool Equals(TType left, TType right) => VogenDefaults<TType, TValueType>.Equals(left, right);
    public static bool Equals(TType left, TType right, IEqualityComparer<TType> comparer) => VogenDefaults<TType, TValueType>.Equals(left, right, comparer);

    public static int CompareTo(TType left, TType right) => VogenDefaults<TType, TValueType>.CompareTo(left, right);
}