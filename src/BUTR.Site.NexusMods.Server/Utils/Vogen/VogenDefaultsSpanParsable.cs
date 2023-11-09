using System;
using System.Diagnostics.CodeAnalysis;

namespace BUTR.Site.NexusMods.Server.Utils.Vogen;

public static class VogenDefaultsSpanParsable<TVogen, TValueObject>
    where TVogen : IVogen<TVogen, TValueObject>, IEquatable<TVogen>, IEquatable<TValueObject>, IComparable<TVogen>
    where TValueObject : ISpanParsable<TValueObject>
{
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, [NotNullWhen(true)] out TVogen? result)
    {
        if(TValueObject.TryParse(s, provider, out var r))
        {
            result = TVogen.From(r);
            return true;
        }

        result = default;
        return false;
    }

    public static bool TryParse(ReadOnlySpan<char> s, [NotNullWhen(true)] out TVogen? result)
    {
        if(TValueObject.TryParse(s, null, out var r))
        {
            result = TVogen.From(r);
            return true;
        }

        result = default;
        return false;
    }
}