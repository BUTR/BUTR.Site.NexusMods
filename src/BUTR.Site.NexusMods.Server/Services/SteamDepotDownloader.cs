using BUTR.Site.NexusMods.Server.Options;

using Microsoft.Extensions.Options;

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services
{
    public sealed class SteamDepotDownloader
    {
        private readonly SteamDepotDownloaderOptions _options;

        public SteamDepotDownloader(IOptions<SteamDepotDownloaderOptions> options)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task DownloadAsync(string version, string path, CancellationToken ct)
        {
            var args = $"{_options.BinaryPath} -app {_options.AppId} -depot {string.Join(" ", _options.Depots)} -beta {version} -filelist {_options.Filelist} -username {_options.Username} -password {_options.Password} -dir {path}";
            var processInfo = new ProcessStartInfo("dotnet", args);
            var process = Process.Start(processInfo);
            await process.WaitForExitAsync(ct);
        }
    }
}