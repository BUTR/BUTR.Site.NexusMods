namespace BUTR.Site.NexusMods.Server.Models;

using TType = CrashReportVersion;
using TValueType = Byte;

[ValueObject<TValueType>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson | Conversions.TypeConverter)]
public readonly partial struct CrashReportVersion;