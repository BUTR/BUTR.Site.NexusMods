using System;

using Vogen;

namespace BUTR.Site.NexusMods.Server.Models;

[ValueObject<string>(conversions: Conversions.Default | Conversions.EfCoreValueConverter | Conversions.SystemTextJson)]
//[StronglyTypedId(StronglyTypedIdBackingType.String, StronglyTypedIdConverter.Default | StronglyTypedIdConverter.SystemTextJson | StronglyTypedIdConverter.EfCoreValueConverter)]
public readonly partial struct CrashReportUrl
{
    public static CrashReportUrl From(Uri uri) => From(uri.ToString());
}