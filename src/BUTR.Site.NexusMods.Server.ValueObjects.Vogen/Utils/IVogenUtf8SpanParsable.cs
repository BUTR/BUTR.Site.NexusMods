namespace BUTR.Site.NexusMods.Server.ValueObjects.Utils;

public interface IVogenUtf8SpanParsable<TVogen, TValueObject>
    where TVogen : IVogen<TVogen, TValueObject>
    where TValueObject : IUtf8SpanParsable<TValueObject>
{
    static abstract bool TryParse(string s, IFormatProvider provider, [NotNullWhen(true)] out TVogen? result);
    static abstract bool TryParse(string s, [NotNullWhen(true)] out TVogen? result);
}