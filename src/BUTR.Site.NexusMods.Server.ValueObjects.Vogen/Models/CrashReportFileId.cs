namespace BUTR.Site.NexusMods.Server.Models;

using TType = CrashReportFileId;
using TValueType = String;

[ValueObject<TValueType>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson | Conversions.TypeConverter)]
public readonly partial struct CrashReportFileId;