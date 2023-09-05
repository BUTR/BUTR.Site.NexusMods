using Vogen;

namespace BUTR.Site.NexusMods.Server.Models;

[ValueObject<string>(conversions: Conversions.Default | Conversions.EfCoreValueConverter | Conversions.SystemTextJson)]
public readonly partial struct ExceptionTypeId { }