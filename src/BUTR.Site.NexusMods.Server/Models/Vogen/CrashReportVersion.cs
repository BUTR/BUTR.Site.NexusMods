using Vogen;

namespace BUTR.Site.NexusMods.Server.Models;

[ValueObject<byte>(conversions: Conversions.Default | Conversions.EfCoreValueConverter | Conversions.SystemTextJson)]
public readonly partial struct CrashReportVersion { }