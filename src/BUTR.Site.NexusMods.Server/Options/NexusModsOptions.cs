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
        public string Endpoint { get; init; } = default!;
        public string APIEndpoint { get; init; } = default!;
        public string ApiKey { get; init; } = default!;
    }
}