namespace BUTR.Site.NexusMods.Server.ValueObjects.Utils;

public sealed class VogenValueConverter<TVogen, TValueObject> : ValueConverter<TVogen, TValueObject>
    where TVogen : struct, IVogen<TVogen, TValueObject>, IHasIsInitialized<TVogen>, IEquatable<TVogen>, IEquatable<TValueObject>, IComparable<TVogen>, IComparable
    where TValueObject : notnull
{
    public VogenValueConverter() : this(null) { }
    public VogenValueConverter(ConverterMappingHints? mappingHints = null) : base(
        vo => VogenDefaults<TVogen, TValueObject>.Convert(vo),
        value => VogenDefaults<TVogen, TValueObject>.Deserialize(value), mappingHints)
    { }
}