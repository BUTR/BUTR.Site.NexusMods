using System;

using Vogen;

namespace BUTR.Site.NexusMods.Server.Models;

[ValueObject<Guid>(conversions: Conversions.Default | Conversions.EfCoreValueConverter | Conversions.SystemTextJson)]
//[StronglyTypedId(StronglyTypedIdBackingType.Guid, StronglyTypedIdConverter.Default | StronglyTypedIdConverter.SystemTextJson | StronglyTypedIdConverter.EfCoreValueConverter)]
public readonly partial struct CrashReportId { }