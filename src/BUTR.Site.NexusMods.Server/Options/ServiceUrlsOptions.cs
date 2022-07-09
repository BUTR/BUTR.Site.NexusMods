using Aragas.Extensions.Options.FluentValidation.Extensions;

using FluentValidation;

using System.Net.Http;

namespace BUTR.Site.NexusMods.Server.Options
{
    public sealed class ServiceUrlsOptionsValidator : AbstractValidator<ServiceUrlsOptions>
    {
        public ServiceUrlsOptionsValidator(HttpClient client)
        {
            RuleFor(x => x.NexusMods).NotEmpty().IsUri();
        }
    }

    public sealed record ServiceUrlsOptions
    {
        public string NexusMods { get; init; } = default!;
    }
}