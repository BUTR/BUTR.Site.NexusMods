using BUTR.Site.NexusMods.Shared.Helpers;

using Vogen;

namespace BUTR.Site.NexusMods.Server.Models;

[ValueObject<string>(conversions: Conversions.Default | Conversions.EfCoreValueConverter | Conversions.SystemTextJson)]
[Instance("Anonymous", ApplicationRoles.Anonymous)]
[Instance("User", ApplicationRoles.User)]
[Instance("Moderator", ApplicationRoles.Moderator)]
[Instance("Administrator", ApplicationRoles.Administrator)]
public readonly partial struct ApplicationRole { }