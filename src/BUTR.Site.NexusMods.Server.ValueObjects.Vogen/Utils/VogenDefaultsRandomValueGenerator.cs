namespace BUTR.Site.NexusMods.Server.ValueObjects.Utils;

public static class VogenDefaultsRandomValueGenerator<TValue, TValueObject, TRandom>
    where TValue : IVogen<TValue, TValueObject>, IHasRandomValueGenerator<TValue, TValueObject, TRandom>
    where TValueObject : unmanaged
    where TRandom : Random, new()
{
    public static TValue NewRandomValue()
    {
        var size = Unsafe.SizeOf<TValueObject>();

        var random = new TRandom();
        Span<byte> bytes = stackalloc byte[size];
        random.NextBytes(bytes);

        var id = MemoryMarshal.Cast<byte, TValueObject>(bytes)[0];
        return TValue.From(id);
    }
}