using Aragas.Extensions.Options.FluentValidation.Extensions;

using FluentValidation;

namespace BUTR.Site.NexusMods.Server.Options
{
    public sealed class DiscordOptionsValidator : AbstractValidator<DiscordOptions>
    {
        public DiscordOptionsValidator()
        {
            RuleFor(x => x.ClientId).NotEmpty();
            RuleFor(x => x.ClientSecret).NotEmpty();
            RuleFor(x => x.RedirectUri).NotEmpty().IsUri();
        }
    }

    public sealed record DiscordOptions
    {
        public required string ClientId { get; init; }
        public required string ClientSecret { get; init; }
        public required string RedirectUri { get; init; }
        public required string BotToken { get; init; }
    }
}