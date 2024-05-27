namespace BUTR.Site.NexusMods.Server.Models;

using TType = NexusModsFileId;
using TValueType = Int32;

[ValueObject<TValueType>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson | Conversions.TypeConverter)]
public readonly partial struct NexusModsFileId;