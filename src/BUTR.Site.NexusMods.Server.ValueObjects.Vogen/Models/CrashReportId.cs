namespace BUTR.Site.NexusMods.Server.Models;

using TType = CrashReportId;
using TValueType = Guid;

[ValueObject<TValueType>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson | Conversions.TypeConverter)]
public readonly partial struct CrashReportId : IHasRandomValueGenerator<TType, TValueType, Random>
{
    public static TType NewRandomValue(Random? random) => From(TValueType.NewGuid());
}