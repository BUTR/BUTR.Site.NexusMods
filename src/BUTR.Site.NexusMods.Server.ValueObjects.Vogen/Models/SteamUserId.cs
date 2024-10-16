namespace BUTR.Site.NexusMods.Server.Models;

using TType = SteamUserId;
using TValueType = String;

[ValueObject<TValueType>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson | Conversions.TypeConverter)]
public readonly partial struct SteamUserId : IHasDefaultValue<TType>
{
    public static readonly TType None = new(string.Empty);

    public static TType DefaultValue => None;

    public static bool TryParse(TValueType url, out TType steamId)
    {
        steamId = None;

        if (!url.Contains("steamcommunity.com/", StringComparison.OrdinalIgnoreCase))
            return false;

        var str1 = url.ToLowerInvariant().Split("steamcommunity.com/");
        if (str1.Length != 2)
            return false;

        var split = str1[1].Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (split.Length != 3)
            return false;

        steamId = From(split[2]);
        return true;
    }
}