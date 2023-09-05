using Vogen;

namespace BUTR.Site.NexusMods.Server.Models;

[ValueObject<int>(conversions: Conversions.Default | Conversions.EfCoreValueConverter | Conversions.SystemTextJson)]
public readonly partial struct NexusModsFileId { }