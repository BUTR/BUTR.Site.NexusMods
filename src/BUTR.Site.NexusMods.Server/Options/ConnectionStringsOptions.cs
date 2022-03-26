using FluentValidation;

using System.Net.Http;

namespace BUTR.Site.NexusMods.Server.Options
{
    public sealed class ConnectionStringsOptionsValidator : AbstractValidator<ConnectionStringsOptions>
    {
        public ConnectionStringsOptionsValidator(HttpClient client)
        {
            RuleFor(x => x.Main).NotEmpty();
        }
    }

    public record ConnectionStringsOptions
    {
        public string Main { get; init; } = default!;
    }
}