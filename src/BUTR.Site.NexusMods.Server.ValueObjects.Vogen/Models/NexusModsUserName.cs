namespace BUTR.Site.NexusMods.Server.Models;

using TType = NexusModsUserName;
using TValueType = String;

[ValueObject<TValueType>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson | Conversions.TypeConverter)]
public readonly partial struct NexusModsUserName : IHasDefaultValue<TType>
{
    public static readonly TType Empty = From(string.Empty);

    public static TType DefaultValue => Empty;

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
}