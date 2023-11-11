using Bannerlord.ModuleManager;

using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.NexusModsAPI;
using BUTR.Site.NexusMods.Server.Utils.Http;

using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace BUTR.Site.NexusMods.Server.Services;

public sealed record NexusModsModFileParserResult
{
    public required ModuleInfoExtended ModuleInfo { get; init; }
    public required NexusModsFileId FileId { get; init; }
    public required DateTimeOffset Uploaded { get; init; }
}

public class NexusModsModFileParser
{
    private readonly HttpClient _httpClient;
    private readonly NexusModsAPIClient _apiClient;

    public NexusModsModFileParser(HttpClient httpClient, NexusModsAPIClient apiClient)
    {
        _httpClient = httpClient;
        _apiClient = apiClient;
    }

    public async IAsyncEnumerable<NexusModsModFileParserResult> GetModuleInfosAsync(NexusModsGameDomain gameDomain, NexusModsModId modId, IEnumerable<NexusModsModFilesResponse.File> files, NexusModsApiKey apiKey, [EnumeratorCancellation] CancellationToken ct)
    {
        const int DefaultBufferSize = 1024 * 16;
        const int LargeBufferSize = 1024 * 1024 * 5;

        foreach (var fileInfo in files)
        {
            if (fileInfo.CategoryName is null) continue;
            if (fileInfo.FileId is not { } fileId) continue;
            if (fileInfo.UploadedTimestamp is not { } uploadedTimestampRaw) continue;

            if (await SubModuleXmlCountAsync(fileInfo) is var subModuleCount && false || subModuleCount == 0) continue;

            var uploadedTimestamp = DateTimeOffset.FromUnixTimeSeconds(uploadedTimestampRaw).ToUniversalTime();
            var downloadLinks = await _apiClient.GetModFileLinksAsync(gameDomain, modId, fileId, apiKey, ct) ?? Array.Empty<NexusModsDownloadLinkResponse>();
            if (downloadLinks.Length == 0) continue;

            await using var httpStream = HttpRangeStream.CreateOrDefault(downloadLinks.OrderByDescending(x => x.Url.Contains("cf-files")).Select(x => new Uri(x.Url)).ToArray(), _httpClient, new HttpRangeOptions { BufferSize = DefaultBufferSize });
            if (httpStream is null) throw new InvalidOperationException($"Failed to get HttpStream for file '{fileInfo.FileName}'");

            using var archive = ArchiveExtensions.OpenOrDefault(httpStream, new ReaderOptions { LeaveStreamOpen = true });
            if (archive is null) throw new InvalidOperationException($"Failed to get Archive for file '{fileInfo.FileName}'");

            if (archive is { IsSolid: true, Type: ArchiveType.Rar })
            {
                httpStream.SetBufferSize(LargeBufferSize);
                using var reader = ReaderExtensions.OpenOrDefault(httpStream, new ReaderOptions { LeaveStreamOpen = true });
                if (reader is null) throw new InvalidOperationException($"Failed to get Reader for file '{fileInfo.FileName}'");
                await foreach (var moduleInfo in GetModuleInfosFromReaderAsync(reader, subModuleCount).WithCancellation(ct))
                    yield return new() { ModuleInfo = moduleInfo, FileId = fileId, Uploaded = uploadedTimestamp };
                continue;
            }

            if (archive.Type == ArchiveType.SevenZip)
                httpStream.SetBufferSize(LargeBufferSize);

            if (archive.Type == ArchiveType.Rar)
                httpStream.SetBufferSize(LargeBufferSize);

            await foreach (var moduleInfo in GetModuleInfosFromArchiveAsync(archive, subModuleCount).WithCancellation(ct))
                yield return new() { ModuleInfo = moduleInfo, FileId = fileId, Uploaded = uploadedTimestamp };
        }
    }

    private static async IAsyncEnumerable<ModuleInfoExtended> GetModuleInfosFromReaderAsync(IReader reader, int subModuleCount)
    {
        while (subModuleCount > 0 && reader.MoveToNextEntry())
        {
            if (reader.Entry.IsDirectory) continue;

            if (!reader.Entry.Key.Contains("SubModule.xml", StringComparison.OrdinalIgnoreCase)) continue;

            await using var stream = reader.OpenEntryStream();
            if (GetModuleInfo(stream) is not { } moduleInfo) continue;

            yield return moduleInfo;
            subModuleCount--;
        }
    }

    private static async IAsyncEnumerable<ModuleInfoExtended> GetModuleInfosFromArchiveAsync(IArchive archive, int subModuleCount)
    {
        foreach (var entry in archive.Entries)
        {
            if (subModuleCount > 0) break;

            if (entry.IsDirectory) continue;

            if (!entry.Key.Contains("SubModule.xml", StringComparison.OrdinalIgnoreCase)) continue;

            await using var stream = entry.OpenEntryStream();
            if (GetModuleInfo(stream) is not { } moduleInfo) continue;

            yield return moduleInfo;
            subModuleCount--;
        }
    }

    private async Task<int> SubModuleXmlCountAsync(NexusModsModFilesResponse.File fileInfo)
    {
        static int ContainsSubModuleFile(IReadOnlyList<NexusModsModFileContentResponse.ContentEntry>? entries)
        {
            if (entries is null)
                return 0;

            var count = 0;
            foreach (var entry in entries)
            {
                if (entry.Name.Equals("SubModule.xml", StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                    continue;
                }

                count += ContainsSubModuleFile(entry.Children);
            }
            return count;
        }

        try
        {
            var content = await _httpClient.GetFromJsonAsync<NexusModsModFileContentResponse>(fileInfo.ContentPreviewLink);
            if (content is null) return 0;
            return ContainsSubModuleFile(content.Children);
        }
        catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            return 0;
        }
    }

    private static ModuleInfoExtended? GetModuleInfo(Stream stream)
    {
        try
        {
            var document = new XmlDocument();
            document.Load(stream);

            return ModuleInfoExtended.FromXml(document);
        }
        catch (Exception)
        {
            return null;
        }
    }
}