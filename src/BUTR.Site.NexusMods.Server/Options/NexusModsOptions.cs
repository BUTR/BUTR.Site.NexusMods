using Aragas.Extensions.Options.FluentValidation.Extensions;

using FluentValidation;

using System.Net.Http;

namespace BUTR.Site.NexusMods.Server.Options
{
    public sealed class NexusModsOptionsValidator : AbstractValidator<NexusModsOptions>
    {
        public NexusModsOptionsValidator(HttpClient client)
        {
            RuleFor(x => x.Endpoint).NotEmpty().IsUri();
            RuleFor(x => x.APIEndpoint).NotEmpty().IsUri();
            RuleFor(x => x.ApiKey).NotEmpty();
        }
    }

    public sealed record NexusModsOptions
    {
        public required string Endpoint { get; init; }
        public required string APIEndpoint { get; init; }
        public required string ApiKey { get; init; }
    }
}