using Aragas.Extensions.Options.FluentValidation.Extensions;

using FluentValidation;

namespace BUTR.Site.NexusMods.Server.Options;

public sealed class SteamAPIOptionsValidator : AbstractValidator<SteamAPIOptions>
{
    public SteamAPIOptionsValidator()
    {
        RuleFor(x => x.APIKey).NotEmpty();
        RuleFor(x => x.Realm).NotEmpty().IsUri();
        RuleFor(x => x.RedirectUri).NotEmpty().IsUri();
    }
}

public sealed record SteamAPIOptions
{
    public required string APIKey { get; init; }
    public required string Realm { get; init; }
    public required string RedirectUri { get; init; }
}