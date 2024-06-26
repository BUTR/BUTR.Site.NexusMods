using Aragas.Extensions.Options.FluentValidation.Extensions;

using FluentValidation;

using System.Net.Http;

namespace BUTR.Site.NexusMods.Server.Options;

public sealed class NexusModsUsersOptionsValidator : AbstractValidator<NexusModsUsersOptions>
{
    public NexusModsUsersOptionsValidator(HttpClient client)
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.RedirectUri).NotEmpty().IsUri();
    }
}

public sealed record NexusModsUsersOptions
{
    public required string ClientId { get; init; }
    public required string RedirectUri { get; init; }
}