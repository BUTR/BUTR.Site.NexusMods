namespace BUTR.Site.NexusMods.Server.ValueObjects.Utils;

public sealed class VogenValueComparerReference<TVogen, TValueObject> : ValueComparer<TVogen>
    where TVogen : IVogen<TVogen, TValueObject>, IEquatable<TVogen>, IEquatable<TValueObject>, IComparable<TVogen>, IComparable
    where TValueObject : notnull
{
    public VogenValueComparerReference() : base(
        CreateDefaultEqualsExpression(),
        instance => VogenDefaults<TVogen, TValueObject>.GetHashCode(instance),
        instance => VogenDefaults<TVogen, TValueObject>.Copy(instance))
    { }

    public override int GetHashCode(TVogen instance) => VogenDefaults<TVogen, TValueObject>.GetHashCode(instance);
    public override TVogen Snapshot(TVogen instance) => VogenDefaults<TVogen, TValueObject>.Copy(instance);
}