namespace BUTR.Site.NexusMods.Server.ValueObjects.Utils;

public interface IVogenParsable<TVogen, TValueObject>
    where TVogen : IVogen<TVogen, TValueObject>
    where TValueObject : IParsable<TValueObject>
{
    static abstract bool TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider provider, [NotNullWhen(true)] out TVogen? result);
    static abstract bool TryParse(ReadOnlySpan<byte> utf8Text, [NotNullWhen(true)] out TVogen? result);
}