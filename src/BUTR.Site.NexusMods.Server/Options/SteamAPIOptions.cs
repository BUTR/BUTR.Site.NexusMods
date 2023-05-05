using FluentValidation;

namespace BUTR.Site.NexusMods.Server.Options;

public sealed class SteamAPIOptionsValidator : AbstractValidator<SteamAPIOptions>
{
    public SteamAPIOptionsValidator()
    {
        RuleFor(x => x.APIKey).NotEmpty();
    }
}

public sealed record SteamAPIOptions
{
    public required string APIKey { get; init; }
}