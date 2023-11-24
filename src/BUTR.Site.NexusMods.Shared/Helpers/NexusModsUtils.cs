using System;
using System.Diagnostics.CodeAnalysis;

namespace BUTR.Site.NexusMods.Shared.Helpers;

public static class NexusModsUtils
{
    public static bool TryParse(string? url, [NotNullWhen(true)] out string? gameDomain, out uint modId)
    {
        gameDomain = default;
        modId = default;

        if (url is null)
            return false;

        if (!url.Contains("nexusmods.com/"))
            return false;

        var str1 = url.Split("nexusmods.com/");
        if (str1.Length != 2)
            return false;

        var split = str1[1].Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (split.Length != 3)
            return false;

        if (!uint.TryParse(split[2], out var modIdNumber))
            return false;

        gameDomain = split[0];
        modId = modIdNumber;
        return true;
    }
}