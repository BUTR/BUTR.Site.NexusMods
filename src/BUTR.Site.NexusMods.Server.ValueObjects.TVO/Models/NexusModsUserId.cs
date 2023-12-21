namespace BUTR.Site.NexusMods.Server.Models;

using TType = NexusModsUserId;
using TValueType = Int32;

[ValueObject<TValueType>]
public readonly partial struct NexusModsUserId : IAugmentWith<DefaultValueAugment, JsonAugment, EfCoreAugment>
{
    public static readonly TType None = From(0);

    public static TType DefaultValue => None;

    public static bool TryParse(string userIdRaw, out TType userId)
    {
        var result = TValueType.TryParse(userIdRaw, out var userIdVal);
        userId = result ? From(userIdVal) : DefaultValue;
        return result;
    }

    public static bool TryParseUrl(string? urlRaw, out TType userId)
    {
        userId = DefaultValue;

        if (!Uri.TryCreate(urlRaw, UriKind.Absolute, out var url))
            return false;

        if (!url.Host.EndsWith("nexusmods.com"))
            return false;

        if (url.LocalPath.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) is not [_, _, var userIdRaw, ..])
            return false;

        return TryParse(userIdRaw, out userId);
    }

    public static TType From(uint id) => From((TValueType) id);
}

public static class NexusModsUserIdExtension
{
    public static PropertyBuilder<TType> HasValueObjectConversion(this PropertyBuilder<TType> propertyBuilder) => propertyBuilder
        .HasConversion<TType.EfCoreValueConverter, TType.EfCoreValueComparer>();
}