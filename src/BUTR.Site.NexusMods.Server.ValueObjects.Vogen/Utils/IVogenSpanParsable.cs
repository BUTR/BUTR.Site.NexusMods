namespace BUTR.Site.NexusMods.Server.ValueObjects.Utils;

public interface IVogenSpanParsable<TVogen, TValueObject>
    where TVogen : IVogen<TVogen, TValueObject>
    where TValueObject : ISpanParsable<TValueObject>
{
    static abstract bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, [NotNullWhen(true)] out TVogen? result);
    static abstract bool TryParse(ReadOnlySpan<char> s, [NotNullWhen(true)] out TVogen? result);
}