namespace BUTR.Site.NexusMods.Server.ValueObjects.Utils;

public sealed class VogenValueComparer<TVogen, TValueObject> : ValueComparer<TVogen>
    where TVogen : struct, IVogen<TVogen, TValueObject>, IHasIsInitialized<TVogen>, IEquatable<TVogen>, IEquatable<TValueObject>, IComparable<TVogen>, IComparable
    where TValueObject : notnull
{
    public VogenValueComparer() : base(
        (left, right) => VogenDefaults<TVogen, TValueObject>.Equals(left, right),
        instance => VogenDefaults<TVogen, TValueObject>.GetHashCode(instance),
        instance => VogenDefaults<TVogen, TValueObject>.Copy(instance))
    { }

    public override int GetHashCode(TVogen instance) => VogenDefaults<TVogen, TValueObject>.GetHashCode(instance);
    public override bool Equals(TVogen left, TVogen right) => VogenDefaults<TVogen, TValueObject>.Equals(left, right);
    public override TVogen Snapshot(TVogen instance) => VogenDefaults<TVogen, TValueObject>.Copy(instance);
}