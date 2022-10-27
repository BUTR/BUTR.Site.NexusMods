using FluentValidation;

namespace BUTR.Site.NexusMods.Server.Options
{
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
        public string BinaryPath { get; set; } = default!;
        public int AppId { get; set; } = default!;
        public int[] Depots { get; set; } = default!;
        public string Filelist { get; set; } = default!;
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}