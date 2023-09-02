using System;
using System.Diagnostics.CodeAnalysis;

namespace BUTR.Site.NexusMods.Shared.Helpers;

public static class SteamUtils
{
    public static bool TryParse(string url, [NotNullWhen(true)] out string? steamId)
    {
        steamId = default;

        if (!url.Contains("steamcommunity.com/"))
            return false;

        var str1 = url.Split("steamcommunity.com/");
        if (str1.Length != 2)
            return false;

        var split = str1[1].Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (split.Length != 3)
            return false;

        steamId = split[2];
        return true;
    }
}