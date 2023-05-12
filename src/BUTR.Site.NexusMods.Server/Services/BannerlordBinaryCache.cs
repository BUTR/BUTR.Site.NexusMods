using BUTR.Site.NexusMods.Server.Options;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services;

public sealed class BannerlordBinaryCache
{
    private readonly SteamDepotDownloader _steamDepotDownloader;
    private readonly SteamDepotDownloaderOptions _options;
    private readonly IDistributedCache _distributedCache;

    public BannerlordBinaryCache(SteamDepotDownloader steamDepotDownloader, IOptions<SteamDepotDownloaderOptions> options, IDistributedCache distributedCache)
    {
        _steamDepotDownloader = steamDepotDownloader;
        _options = options.Value;
        _distributedCache = distributedCache;
    }

    public async Task<IEnumerable<string>> GetBranchAssemblyFiles(string branch, CancellationToken ct)
    {
        var path = Path.Combine(_options.DownloadPath, branch);

        if (await _distributedCache.GetStringAsync(path, ct) is { } filesRaw)
            return filesRaw.Split(';');

        await _steamDepotDownloader.DownloadAsync(branch, path, CancellationToken.None);
        var files = Directory.EnumerateFiles(path, "*.dll", SearchOption.AllDirectories).ToArray();

        await _distributedCache.SetStringAsync(path, string.Join(';', files), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(8)
        }, token: ct);
        return files;
    }
}