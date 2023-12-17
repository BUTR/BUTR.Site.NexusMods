namespace BUTR.Site.NexusMods.Server.ValueObjects.Utils;

internal class TransparentValueObjectTypeConverter<TValueType, TInnerValue> : TypeConverter
    where TValueType : struct, IValueObjectFrom<TValueType, TInnerValue>
    where TInnerValue : notnull
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        sourceType == typeof(TInnerValue) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
        value is TInnerValue valValue ? TValueType.From(valValue) : base.ConvertFrom(context, culture, value);

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? sourceType) =>
        sourceType == typeof(TValueType) || base.CanConvertTo(context, sourceType);

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) => value is TValueType idValue
        ? destinationType == typeof(TInnerValue) ? (TInnerValue) idValue : base.ConvertTo(context, culture, value, destinationType)
        : base.ConvertTo(context, culture, value, destinationType);
}