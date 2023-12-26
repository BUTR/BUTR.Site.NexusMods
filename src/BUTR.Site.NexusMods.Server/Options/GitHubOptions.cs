using Aragas.Extensions.Options.FluentValidation.Extensions;

using FluentValidation;

using System.Net.Http;

namespace BUTR.Site.NexusMods.Server.Options;

public sealed class GitHubOptionsValidator : AbstractValidator<GitHubOptions>
{
    public GitHubOptionsValidator(HttpClient client)
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.ClientSecret).NotEmpty();
        RuleFor(x => x.RedirectUri).NotEmpty().IsUri();
    }
}

public sealed record GitHubOptions
{
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public required string RedirectUri { get; init; }
}