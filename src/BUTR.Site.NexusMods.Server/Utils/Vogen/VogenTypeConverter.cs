using System;
using System.ComponentModel;
using System.Globalization;

namespace BUTR.Site.NexusMods.Server.Utils.Vogen;

public class VogenTypeConverter<TVogen, TValueObject> : TypeConverter
    where TVogen : struct, IVogen<TVogen, TValueObject>, IHasIsInitialized<TVogen>, IEquatable<TVogen>, IEquatable<TValueObject>, IComparable<TVogen>
    where TValueObject : notnull
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        sourceType == typeof(TValueObject) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
        value is TValueObject valValue ? TVogen.DeserializeDangerous(valValue) : base.ConvertFrom(context, culture, value);

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? sourceType) =>
        sourceType == typeof(TVogen) || base.CanConvertTo(context, sourceType);

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) => value is TVogen idValue
        ? destinationType == typeof(TValueObject) ? (TValueObject) idValue : base.ConvertTo(context, culture, value, destinationType)
        : base.ConvertTo(context, culture, value, destinationType);
}