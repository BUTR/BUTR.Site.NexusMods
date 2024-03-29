using FluentValidation;

namespace BUTR.Site.NexusMods.Server.Options;

public sealed class JwtOptionsValidator : AbstractValidator<JwtOptions>
{
    public JwtOptionsValidator()
    {
        RuleFor(x => x.SignKey).Length(16);
        RuleFor(x => x.EncryptionKey).Length(16);
    }
}

public sealed record JwtOptions
{
    public required string SignKey { get; init; }
    public required string EncryptionKey { get; init; }
}