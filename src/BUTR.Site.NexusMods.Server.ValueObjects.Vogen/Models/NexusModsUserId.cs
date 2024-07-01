namespace BUTR.Site.NexusMods.Server.Models;

using TType = NexusModsUserId;
using TValueType = Int32;

[ValueObject<TValueType>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson | Conversions.TypeConverter)]
public readonly partial struct NexusModsUserId : IHasDefaultValue<TType>
{
    public static readonly TType None = new(0);

    public static TType DefaultValue => None;

    public static bool TryParseUrl(string urlRaw, out TType userId)
    {
        userId = DefaultValue;

        if (!Uri.TryCreate(urlRaw, UriKind.Absolute, out var url))
            return false;

        if (!url.Host.EndsWith("nexusmods.com"))
            return false;

        if (url.LocalPath.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) is not [_, var users, var userIdRaw, ..])
            return false;

        if (!string.Equals(users, "users", StringComparison.OrdinalIgnoreCase))
            return false;
        
        return TryParse(userIdRaw, out userId);
    }

    public static TType From(uint id) => From((TValueType) id);
}