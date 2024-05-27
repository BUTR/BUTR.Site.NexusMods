namespace BUTR.Site.NexusMods.Server.Models;

using TType = ApplicationRole;
using TValueType = String;

[ValueObject<TValueType>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson | Conversions.TypeConverter, deserializationStrictness: DeserializationStrictness.AllowKnownInstances)]
public partial struct ApplicationRole : IHasDefaultValue<TType>
{
    public static readonly TType Anonymous = new(ApplicationRoles.Anonymous);
    public static readonly TType User = new(ApplicationRoles.User);
    public static readonly TType Moderator = new(ApplicationRoles.Moderator);
    public static readonly TType Administrator = new(ApplicationRoles.Administrator);

    public static TType DefaultValue => Anonymous;
}