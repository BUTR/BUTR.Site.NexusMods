namespace BUTR.Site.NexusMods.Server.Models;

using TType = NexusModsUserEMail;
using TValueType = String;

[ValueObject<TValueType>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson | Conversions.TypeConverter)]
public readonly partial struct NexusModsUserEMail : IHasDefaultValue<TType>
{
    public static readonly TType Empty = From(string.Empty);

    public static TType DefaultValue => Empty;
}