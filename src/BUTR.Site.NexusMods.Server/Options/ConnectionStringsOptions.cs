using FluentValidation;

using System.Net.Http;

namespace BUTR.Site.NexusMods.Server.Options;

public sealed class ConnectionStringsOptionsValidator : AbstractValidator<ConnectionStringsOptions>
{
    public ConnectionStringsOptionsValidator()
    {
        RuleFor(x => x.Main).NotEmpty();
    }
}

public sealed record ConnectionStringsOptions
{
    public required string Main { get; init; }
}