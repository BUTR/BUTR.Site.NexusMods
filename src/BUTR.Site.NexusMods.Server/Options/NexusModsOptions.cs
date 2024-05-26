using BUTR.Site.NexusMods.Server.Models;

using FluentValidation;

namespace BUTR.Site.NexusMods.Server.Options;

public sealed class NexusModsOptionsValidator : AbstractValidator<NexusModsOptions>
{
    public NexusModsOptionsValidator()
    {
        RuleFor(x => x.ApiKey).NotEmpty();
    }
}

public sealed record NexusModsOptions
{
    public required NexusModsApiKey ApiKey { get; init; } = NexusModsApiKey.None;
}