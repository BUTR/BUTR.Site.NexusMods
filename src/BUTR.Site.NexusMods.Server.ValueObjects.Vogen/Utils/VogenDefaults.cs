namespace BUTR.Site.NexusMods.Server.ValueObjects.Utils;

public static class VogenDefaults<TVogen, TValueObject>
    where TVogen : IVogen<TVogen, TValueObject>, IEquatable<TVogen>, IEquatable<TValueObject>, IComparable<TVogen>, IComparable
    where TValueObject : notnull
{
    public static TVogen Copy(TVogen value) => TVogen.Copy(value);

    public static TVogen Deserialize(TValueObject value) => TVogen.DeserializeDangerous(value);

    public static TValueObject Convert(TVogen value) => (TValueObject) value;
    public static TVogen Convert(TValueObject value) => (TVogen) value;

    public static int GetHashCode(TVogen value) => TVogen.IsInitialized(value) ? value.GetHashCode() : 0;

    public static bool Equals(TVogen left, TVogen right)
    {
        var leftInitialized = TVogen.IsInitialized(left);
        var rightInitialized = TVogen.IsInitialized(right);
        return (!leftInitialized && !rightInitialized) || (leftInitialized && rightInitialized && left.Equals(right));
    }

    public static bool Equals(TVogen left, TVogen right, IEqualityComparer<TVogen> comparer)
    {
        var leftInitialized = TVogen.IsInitialized(left);
        var rightInitialized = TVogen.IsInitialized(right);
        return (!leftInitialized && !rightInitialized) || (leftInitialized && rightInitialized && comparer.Equals(left, right));
    }

    public static int CompareTo(TVogen left, TVogen right)
    {
        var leftInitialized = TVogen.IsInitialized(left);
        var rightInitialized = TVogen.IsInitialized(right);
        return !leftInitialized && !rightInitialized ? 0 : !leftInitialized ? -1 : !rightInitialized ? 1 : left.CompareTo(right);
    }
}