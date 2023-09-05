using System;

using Vogen;

namespace BUTR.Site.NexusMods.Server.Models;

[ValueObject<int>(conversions: Conversions.Default | Conversions.EfCoreValueConverter | Conversions.SystemTextJson)]
public readonly partial struct NexusModsModId
{
    public static bool TryParseUrl(string url, out NexusModsModId modId)
    {
        modId = From(0);

        if (!url.Contains("nexusmods.com/"))
            return false;

        var str1 = url.Split("nexusmods.com/");
        if (str1.Length != 2)
            return false;

        var split = str1[1].Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (split.Length != 3)
            return false;

        return TryParse(split[2], out modId);
    }
}