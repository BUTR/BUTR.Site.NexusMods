using System;

namespace BUTR.Site.NexusMods.Server.Utils.Vogen;

public interface IHasRandomValueGenerator<out TVogen, out TValueObject, in TRandom>
    where TVogen : IVogen<TVogen, TValueObject>
    where TValueObject : notnull
    where TRandom : Random
{
    static abstract TVogen NewRandomValue(TRandom? random);
}