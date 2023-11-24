namespace BUTR.Site.NexusMods.Server.ValueObjects.Utils;

public interface IHasRandomValueGenerator<out TVogen, out TValueObject, in TRandom>
    where TVogen : IVogen<TVogen, TValueObject>
    where TValueObject : notnull
    where TRandom : Random
{
    static abstract TVogen NewRandomValue(TRandom? random);
}