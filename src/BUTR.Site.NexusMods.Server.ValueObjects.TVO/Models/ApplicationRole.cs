namespace BUTR.Site.NexusMods.Server.Models;

using TType = ApplicationRole;
using TValueType = String;

[ValueObject<TValueType>]
public readonly partial struct ApplicationRole : IAugmentWith<DefaultValueAugment, JsonAugment, EfCoreAugment>
{
    public static readonly TType Anonymous = From(ApplicationRoles.Anonymous);
    public static readonly TType User = From(ApplicationRoles.User);
    public static readonly TType Moderator = From(ApplicationRoles.Moderator);
    public static readonly TType Administrator = From(ApplicationRoles.Administrator);

    public static TType DefaultValue => Anonymous;
}

public static class ApplicationRoleExtension
{
    public static PropertyBuilder<TType> HasValueObjectConversion(this PropertyBuilder<TType> propertyBuilder) => propertyBuilder
        .HasConversion<TType.EfCoreValueConverter, TType.EfCoreValueComparer>();
}