namespace BUTR.Site.NexusMods.Server.ValueObjects.Utils;

public static class VogenDefaults<TVogen, TValueObject>
    where TVogen : IVogen<TVogen, TValueObject>, IEquatable<TVogen>, IEquatable<TValueObject>, IComparable<TVogen>, IComparable
{
    public static TValueObject Convert(TVogen value) => (TValueObject) value;
    public static TVogen Convert(TValueObject value) => (TVogen) value;

    public static int GetHashCode(TVogen value) => value.IsInitialized() ? value.GetHashCode() : 0;

    public static bool Equals(TVogen left, TVogen right)
    {
        var leftInitialized = left.IsInitialized();
        var rightInitialized = right.IsInitialized();
        return (!leftInitialized && !rightInitialized) || (leftInitialized && rightInitialized && left.Equals(right));
    }

    public static bool Equals(TVogen left, TVogen right, IEqualityComparer<TVogen> comparer)
    {
        var leftInitialized = left.IsInitialized();
        var rightInitialized = right.IsInitialized();
        return (!leftInitialized && !rightInitialized) || (leftInitialized && rightInitialized && comparer.Equals(left, right));
    }

    public static int CompareTo(TVogen left, TVogen right)
    {
        var leftInitialized = left.IsInitialized();
        var rightInitialized = right.IsInitialized();
        return !leftInitialized && !rightInitialized ? 0 : !leftInitialized ? -1 : !rightInitialized ? 1 : left.CompareTo(right);
    }
}