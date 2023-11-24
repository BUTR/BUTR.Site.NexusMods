namespace BUTR.Site.NexusMods.Server.Models;

internal interface IValueObjectFrom<TValueType, TInnerValue>
    where TValueType : IValueObjectFrom<TValueType, TInnerValue>
{
    static abstract explicit operator TValueType(TInnerValue value);
    static abstract explicit operator TInnerValue(TValueType value);
    static abstract TValueType From(TInnerValue value);
}