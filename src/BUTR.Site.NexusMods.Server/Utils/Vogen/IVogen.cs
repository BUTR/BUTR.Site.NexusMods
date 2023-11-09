using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BUTR.Site.NexusMods.Server.Utils.Vogen;

public interface IVogen<TVogen, TValueObject> : IHasIsInitialized<TVogen>, IHasDeserialize<TVogen, TValueObject>, IHasCopy<TVogen>
    where TVogen : IVogen<TVogen, TValueObject>
    where TValueObject : notnull
{
    static abstract explicit operator TVogen(TValueObject value);
    static abstract explicit operator TValueObject(TVogen value);

    static abstract bool operator ==(TVogen left, TVogen right);
    static abstract bool operator !=(TVogen left, TVogen right);

    static abstract bool operator ==(TVogen left, TValueObject right);
    static abstract bool operator !=(TVogen left, TValueObject right);

    static abstract bool operator ==(TValueObject left, TVogen right);
    static abstract bool operator !=(TValueObject left, TVogen right);

    static abstract TVogen From(TValueObject value);

    static abstract int GetHashCode(TVogen value);

    static abstract bool Equals(TVogen left, TVogen right);
    static abstract bool Equals(TVogen left, TVogen right, IEqualityComparer<TVogen> comparer);

    static abstract int CompareTo(TVogen left, TVogen right);
}


public interface IVogenParsable<TVogen, TValueObject>
    where TVogen : IVogen<TVogen, TValueObject>
    where TValueObject : IParsable<TValueObject>
{
    static abstract bool TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider provider, [NotNullWhen(true)] out TVogen? result);
    static abstract bool TryParse(ReadOnlySpan<byte> utf8Text, [NotNullWhen(true)] out TVogen? result);
}


public interface IVogenSpanParsable<TVogen, TValueObject>
    where TVogen : IVogen<TVogen, TValueObject>
    where TValueObject : ISpanParsable<TValueObject>
{
    static abstract bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, [NotNullWhen(true)] out TVogen? result);
    static abstract bool TryParse(ReadOnlySpan<char> s, [NotNullWhen(true)] out TVogen? result);
}


public interface IVogenUtf8SpanParsable<TVogen, TValueObject>
    where TVogen : IVogen<TVogen, TValueObject>
    where TValueObject : IUtf8SpanParsable<TValueObject>
{
    static abstract bool TryParse(string s, IFormatProvider provider, [NotNullWhen(true)] out TVogen? result);
    static abstract bool TryParse(string s, [NotNullWhen(true)] out TVogen? result);
}