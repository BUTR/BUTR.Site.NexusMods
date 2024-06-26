namespace BUTR.Site.NexusMods.Server.Models;

using TType = NexusModsModId;
using TValueType = Int32;

[ValueObject<TValueType>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson | Conversions.TypeConverter)]
public readonly partial struct NexusModsModId : IHasDefaultValue<TType>
{
    public static readonly TType None = new(0);

    public static TType DefaultValue => None;

    public static bool TryParseUrl(string? urlRaw, out TType modId)
    {
        modId = DefaultValue;

        if (!Uri.TryCreate(urlRaw, UriKind.Absolute, out var url))
            return false;

        if (!url.Host.EndsWith("nexusmods.com"))
            return false;

        if (url.LocalPath.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) is not [_, var mods, var modIdRaw, ..])
            return false;

        if (!string.Equals(mods, "mods", StringComparison.OrdinalIgnoreCase))
            return false;

        return TryParse(modIdRaw, out modId);
    }
}