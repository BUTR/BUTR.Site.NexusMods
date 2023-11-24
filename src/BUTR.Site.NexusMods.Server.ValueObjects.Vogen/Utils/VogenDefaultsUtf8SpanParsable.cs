namespace BUTR.Site.NexusMods.Server.ValueObjects.Utils;

public static class VogenDefaultsUtf8SpanParsable<TVogen, TValueObject>
    where TVogen : IVogen<TVogen, TValueObject>, IEquatable<TVogen>, IEquatable<TValueObject>, IComparable<TVogen>
    where TValueObject : IUtf8SpanParsable<TValueObject>
{
    public static bool TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider provider, [NotNullWhen(true)] out TVogen? result)
    {
        if (TValueObject.TryParse(utf8Text, provider, out var r))
        {
            result = TVogen.From(r);
            return true;
        }

        result = default;
        return false;
    }

    public static bool TryParse(ReadOnlySpan<byte> utf8Text, [NotNullWhen(true)] out TVogen? result)
    {
        if (TValueObject.TryParse(utf8Text, null, out var r))
        {
            result = TVogen.From(r);
            return true;
        }

        result = default;
        return false;
    }
}