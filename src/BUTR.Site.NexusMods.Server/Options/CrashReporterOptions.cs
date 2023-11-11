using Aragas.Extensions.Options.FluentValidation.Extensions;

using FluentValidation;

using System.Net.Http;

namespace BUTR.Site.NexusMods.Server.Options;

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
    public required string Endpoint { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
}