namespace BUTR.Site.NexusMods.Server.Models;

using TType = ApplicationRole;
using TValueType = String;

[ValueObject<TValueType>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson | Conversions.TypeConverter, deserializationStrictness: DeserializationStrictness.AllowKnownInstances)]
public partial struct ApplicationRole : IHasDefaultValue<TType>
{
    public static readonly TType Anonymous = From(ApplicationRoles.Anonymous);
    public static readonly TType User = From(ApplicationRoles.User);
    public static readonly TType Moderator = From(ApplicationRoles.Moderator);
    public static readonly TType Administrator = From(ApplicationRoles.Administrator);

    public static TType DefaultValue => Anonymous;
}