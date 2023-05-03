using System;

namespace BUTR.Site.NexusMods.Shared.Utils;

public static class LocalizationUtils
{
    public static bool IsTranslationString(in ReadOnlySpan<char> span)
    {
        if (span.Length < 3)
            return false;

        if (span[0] != '{' || span[1] != '=')
            return false;

        if (span.IndexOf('}') == -1)
            return false;

        return true;
    }

    public static bool TryParseTranslationString(in ReadOnlySpan<char> span, out string id, out string content)
    {
        id = string.Empty;
        content = string.Empty;

        if (span.Length < 3)
            return false;

        if (span[0] != '{' || span[1] != '=')
            return false;

        if (span.IndexOf('}') is var idx && idx == -1)
            return false;

        id = new string(span.Slice(0, idx + 1));
        content = new string(span.Slice(idx + 1));
        return true;
    }
}