using Aragas.Extensions.Options.FluentValidation.Extensions;

using FluentValidation;

using System.Net.Http;

namespace BUTR.Site.NexusMods.Server.Options
{
    public sealed class CrashReporterOptionsValidator : AbstractValidator<CrashReporterOptions>
    {
        public CrashReporterOptionsValidator(HttpClient client)
        {
            RuleFor(x => x.Endpoint).NotEmpty().IsUri();
            RuleFor(x => x.Username).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
        }
    }
    
    public sealed record CrashReporterOptions
    {
        public string Endpoint { get; init; } = default!;
        public string Username { get; init; } = default!;
        public string Password { get; init; } = default!;
    }
}