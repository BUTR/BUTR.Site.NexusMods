namespace BUTR.Site.NexusMods.Server.ValueObjects.Utils;

public interface IVogen<TVogen, TValueObject>
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
    
    TValueObject Value { get; }

    bool IsInitialized();
}