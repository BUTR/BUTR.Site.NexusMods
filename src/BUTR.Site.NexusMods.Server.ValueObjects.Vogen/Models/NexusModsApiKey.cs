namespace BUTR.Site.NexusMods.Server.Models;

using TType = NexusModsApiKey;
using TValueType = String;

[ValueObject<TValueType>(conversions: Conversions.SystemTextJson | Conversions.TypeConverter)]
public readonly partial struct NexusModsApiKey : IHasDefaultValue<TType>
{
    public static readonly TType None = From(string.Empty);

    public static NexusModsApiKey DefaultValue => None;
}