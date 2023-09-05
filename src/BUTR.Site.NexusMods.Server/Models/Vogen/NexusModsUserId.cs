using System;

using Vogen;

namespace BUTR.Site.NexusMods.Server.Models;

[ValueObject<int>(conversions: Conversions.Default | Conversions.EfCoreValueConverter | Conversions.SystemTextJson)]
[Instance("None", 0)]
public readonly partial struct NexusModsUserId
{
    public static bool TryParseUrl(string url, out NexusModsUserId userId)
    {
        userId = From(0);

        if (!url.Contains("nexusmods.com/"))
            return false;

        var str1 = url.Split("nexusmods.com/");
        if (str1.Length != 2)
            return false;

        var split = str1[1].Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (split.Length != 2)
            return false;

        return TryParse(split[1], out userId);
    }

    public static NexusModsUserId From(uint id) => From((int) id);
}