using Bannerlord.ModuleManager;

using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.NexusModsAPI;
using BUTR.Site.NexusMods.Server.Utils;

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

public class NexusModsModFileParser
{
    private readonly HttpClient _httpClient;
    private readonly NexusModsAPIClient _apiClient;

    public NexusModsModFileParser(HttpClient httpClient, NexusModsAPIClient apiClient)
    {
        _httpClient = httpClient;
        _apiClient = apiClient;
    }


    public async IAsyncEnumerable<ModuleInfoExtended> GetModuleInfosAsync(NexusModsGameDomain gameDomain, NexusModsModId modId, NexusModsApiKey apiKey, [EnumeratorCancellation] CancellationToken ct)
    {
        const int DefaultBufferSize = 1024 * 16;
        const int LargeBufferSize = 1024 * 1024 * 5;

        var fileInfos = await _apiClient.GetModFileInfosAsync(gameDomain, modId, apiKey, ct);
        foreach (var fileInfo in fileInfos?.Files ?? Array.Empty<NexusModsModFilesResponse.File>())
        {
            if (!await HasSubModuleXmlAsync(fileInfo)) continue;

            var downloadLinks = await _apiClient.GetModFileLinksAsync(gameDomain, modId, fileInfo.Id, apiKey, ct) ?? Array.Empty<NexusModsDownloadLinkResponse>();

            await using var httpStream = HttpRangeStream.CreateOrDefault(downloadLinks.Select(x => new Uri(x.Url)).ToArray(), _httpClient, new HttpRangeOptions { BufferSize = DefaultBufferSize });
            if (httpStream is null) throw new InvalidOperationException($"Failed to get HttpStream for file '{fileInfo.FileName}'");

            using var archive = ArchiveExtensions.OpenOrDefault(httpStream, new ReaderOptions { LeaveStreamOpen = true });
            if (archive is null) throw new InvalidOperationException($"Failed to get Archive for file '{fileInfo.FileName}'");

            if (archive is { IsSolid: true, Type: ArchiveType.Rar })
            {
                httpStream.SetBufferSize(LargeBufferSize);
                using var reader = ReaderExtensions.OpenOrDefault(httpStream, new ReaderOptions { LeaveStreamOpen = true });
                if (reader is null) throw new InvalidOperationException($"Failed to get Reader for file '{fileInfo.FileName}'");
                await foreach (var moduleInfo in GetModuleInfosFromReaderAsync(reader).WithCancellation(ct))
                    yield return moduleInfo;
                continue;
            }

            if (archive.Type == ArchiveType.SevenZip)
                httpStream.SetBufferSize(LargeBufferSize);

            if (archive.Type == ArchiveType.Rar)
                httpStream.SetBufferSize(LargeBufferSize);

            await foreach (var moduleInfo in GetModuleInfosFromArchiveAsync(archive).WithCancellation(ct))
                yield return moduleInfo;
        }
    }

    private static async IAsyncEnumerable<ModuleInfoExtended> GetModuleInfosFromReaderAsync(IReader reader)
    {
        while (reader.MoveToNextEntry())
        {
            if (reader.Entry.IsDirectory) continue;

            if (!reader.Entry.Key.Contains("SubModule.xml", StringComparison.OrdinalIgnoreCase)) continue;


            await using var stream = reader.OpenEntryStream();
            if (GetModuleInfo(stream) is not { } moduleInfo) continue;

            yield return moduleInfo;
            break;
        }
    }

    private static async IAsyncEnumerable<ModuleInfoExtended> GetModuleInfosFromArchiveAsync(IArchive archive)
    {
        foreach (var entry in archive.Entries)
        {
            if (entry.IsDirectory) continue;

            if (!entry.Key.Contains("SubModule.xml", StringComparison.OrdinalIgnoreCase)) continue;

            await using var stream = entry.OpenEntryStream();
            if (GetModuleInfo(stream) is not { } moduleInfo) continue;

            yield return moduleInfo;
            break;
        }
    }

    private async Task<bool> HasSubModuleXmlAsync(NexusModsModFilesResponse.File fileInfo)
    {
        static bool ContainsSubModuleFile(IReadOnlyList<NexusModsModFileContentResponse.ContentEntry>? entries)
        {
            if (entries is null)
                return false;

            foreach (var entry in entries)
            {
                if (entry.Name.Equals("SubModule.xml", StringComparison.OrdinalIgnoreCase))
                    return true;
                if (ContainsSubModuleFile(entry.Children))
                    return true;
            }
            return false;
        }

        try
        {
            var content = await _httpClient.GetFromJsonAsync<NexusModsModFileContentResponse>(fileInfo.ContentPreviewUrl);
            if (content is null) return false;
            if (!ContainsSubModuleFile(content.Children)) return false;
        }
        catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
        return true;
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