namespace BUTR.Site.NexusMods.Server.Models;

using TType = GameVersion;
using TValueType = System.String;

[ValueObject<TValueType>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson | Conversions.TypeConverter)]
public readonly partial struct GameVersion;