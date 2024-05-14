using Microsoft.AspNetCore.Routing;

using System.Text.RegularExpressions;

namespace BUTR.Site.NexusMods.Server.Utils;

public partial class SlugifyParameterTransformer : IOutboundParameterTransformer
{
    [GeneratedRegex("([a-z])([A-Z])", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex SlugifyRegex();

    public string? TransformOutbound(object? value)
    {
        if (value == null) return null;

        var str = value.ToString();
        if (string.IsNullOrEmpty(str)) return null;

        return SlugifyRegex().Replace(str, "$1-$2").ToLowerInvariant();
    }
}