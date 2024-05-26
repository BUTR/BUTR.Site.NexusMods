using BUTR.Site.NexusMods.Server.Models;

using FluentValidation;

using System.Net.Http;

namespace BUTR.Site.NexusMods.Server.Options;

public sealed class NexusModsOptionsValidator : AbstractValidator<NexusModsOptions>
{
    public NexusModsOptionsValidator(HttpClient client)
    {
        RuleFor(x => x.ApiKey).NotEmpty();
    }
}

public sealed record NexusModsOptions
{
    // TODO:
    public required string ApiKey { get; init; }
}