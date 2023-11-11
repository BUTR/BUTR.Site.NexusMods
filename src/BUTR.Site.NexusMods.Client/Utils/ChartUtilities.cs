using Blazorise.Charts;

using System;
using System.Collections.Generic;

namespace BUTR.Site.NexusMods.Client.Utils;

internal static class ChartUtiities
{
    public static ChartColor ColorFromHSV(double hue, double saturation, double value, float alpha)
    {
        var hi = ((int) Math.Floor(hue / 60)) % 6;
        var f = hue / 60 - Math.Floor(hue / 60);

        value *= 255;
        var v = (byte) (value);
        var p = (byte) (value * (1 - saturation));
        var q = (byte) (value * (1 - f * saturation));
        var t = (byte) (value * (1 - (1 - f) * saturation));

        return hi switch
        {
            0 => ChartColor.FromRgba(v, t, p, alpha),
            1 => ChartColor.FromRgba(q, v, p, alpha),
            2 => ChartColor.FromRgba(p, v, t, alpha),
            3 => ChartColor.FromRgba(p, q, v, alpha),
            4 => ChartColor.FromRgba(t, p, v, alpha),
            _ => ChartColor.FromRgba(v, p, q, alpha)
        };
    }

    public static IEnumerable<string> GetColors(int n, float alpha)
    {
        if (n == 0)
            yield break;

        var step = 255 / n;
        for (var i = 0; i < n; i++)
        {
            yield return ColorFromHSV(i * step, 1, 1, alpha);
        }
    }
}