using System;
using System.Diagnostics.CodeAnalysis;

namespace BUTR.Site.NexusMods.Shared.Helpers;

public static class NexusModsUtils
{
    public static bool TryParseModUrl(string? url, [NotNullWhen(true)] out string? gameDomain, out uint modId)
    {
        gameDomain = default;
        modId = default;

        if (url is null)
            return false;

        if (!url.Contains("nexusmods.com/", StringComparison.OrdinalIgnoreCase))
            return false;

        var str1 = url.ToLowerInvariant().Split("nexusmods.com/");
        if (str1.Length != 2)
            return false;

        var split = str1[1].Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (split.Length != 3)
            return false;

        if (!string.Equals(split[1], "mods", StringComparison.OrdinalIgnoreCase))
            return false;

        if (!uint.TryParse(split[2], out var modIdNumber))
            return false;

        gameDomain = split[0];
        modId = modIdNumber;
        return true;
    }

    public static bool TryParseUserId(string? url, [NotNullWhen(true)] out string? gameDomain, out uint userId)
    {
        gameDomain = default;
        userId = default;

        if (url is null)
            return false;

        if (!url.Contains("nexusmods.com/", StringComparison.OrdinalIgnoreCase))
            return false;

        var str1 = url.ToLowerInvariant().Split("nexusmods.com/");
        if (str1.Length != 2)
            return false;

        var split = str1[1].Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (split.Length != 3)
            return false;

        if (!string.Equals(split[1], "users", StringComparison.OrdinalIgnoreCase))
            return false;

        if (!uint.TryParse(split[2], out var userIdNumber))
            return false;

        gameDomain = split[0];
        userId = userIdNumber;
        return true;
    }

    public static bool TryParseUsername(string? url, [NotNullWhen(true)] out string? username)
    {
        username = default;

        if (url is null)
            return false;

        if (!url.Contains("nexusmods.com/profile/", StringComparison.OrdinalIgnoreCase))
            return false;

        var str1 = url.ToLowerInvariant().Split("nexusmods.com/profile/");
        if (str1.Length != 2)
            return false;

        var split = str1[1].Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (split.Length < 1)
            return false;

        username = split[0];
        return true;
    }
}