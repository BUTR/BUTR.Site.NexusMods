using FluentValidation;

namespace BUTR.Site.NexusMods.Server.Options;

public sealed class SteamDepotDownloaderOptionsValidator : AbstractValidator<SteamDepotDownloaderOptions>
{
    public SteamDepotDownloaderOptionsValidator()
    {
        RuleFor(x => x.AppId).GreaterThan(0).When(x => !string.IsNullOrEmpty(x.BinaryPath));
        RuleFor(x => x.Depots).NotEmpty().ForEach(x => x.GreaterThan(0)).When(x => !string.IsNullOrEmpty(x.BinaryPath));
        RuleFor(x => x.Filelist).NotEmpty().When(x => !string.IsNullOrEmpty(x.BinaryPath));
        RuleFor(x => x.Username).NotEmpty().When(x => !string.IsNullOrEmpty(x.BinaryPath));
        RuleFor(x => x.Password).NotEmpty().When(x => !string.IsNullOrEmpty(x.BinaryPath));
    }
}

public sealed record SteamDepotDownloaderOptions
{
    public required string BinaryPath { get; init; }
    public required string DownloadPath { get; init; }
    public required int AppId { get; init; }
    public required int[] Depots { get; init; }
    public required string Filelist { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
}