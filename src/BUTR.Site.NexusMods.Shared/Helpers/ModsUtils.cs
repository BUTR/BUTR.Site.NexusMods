using System;
using System.Diagnostics.CodeAnalysis;

namespace BUTR.Site.NexusMods.Shared.Helpers
{
    public static class ModsUtils
    {
        public static bool TryParse(string url, [NotNullWhen(true)] out string? gameDomain, [NotNullWhen(true)] out int? modId)
        {
            gameDomain = null;
            modId = null;

            if (!url.Contains("nexusmods.com/"))
                return false;

            var str1 = url.Split("nexusmods.com/");
            if (str1.Length != 2)
                return false;

            var split = str1[1].Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (split.Length != 3)
                return false;

            if (!int.TryParse(split[2], out var modIdInt))
                return false;

            gameDomain = split[0];
            modId = modIdInt;
            return true;
        }
    }
}