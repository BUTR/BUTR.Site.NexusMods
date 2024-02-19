using AsmResolver.DotNet;
using AsmResolver.PE;

using Bannerlord.ModuleManager;

using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.NexusModsAPI;
using BUTR.Site.NexusMods.Server.Utils;
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
    public required GameVersion[] GameVersions { get; init; }
    public required NexusModsFileId FileId { get; init; }
    public required DateTimeOffset Uploaded { get; init; }
}

public interface INexusModsModFileParser
{
    IAsyncEnumerable<NexusModsModFileParserResult> GetModuleInfosAsync(NexusModsGameDomain gameDomain, NexusModsModId modId, IEnumerable<NexusModsModFilesResponse.File> files, NexusModsApiKey apiKey, CancellationToken ct);
}

public sealed record ModuleInfoExtendedWithPath : ModuleInfoExtended
{
    public string Path { get; init; }

    public ModuleInfoExtendedWithPath(ModuleInfoExtended moduleInfoExtended, string path) : base(moduleInfoExtended)
    {
        Path = path;
    }
}

[TransientService<INexusModsModFileParser>]
public class NexusModsModFileParser : INexusModsModFileParser
{
    private readonly HttpClient _httpClient;
    private readonly INexusModsAPIClient _apiClient;

    public NexusModsModFileParser(HttpClient httpClient, INexusModsAPIClient apiClient)
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

                var moduleInfosReader = await GetModuleInfosFromReaderAsync(reader, subModuleCount).ToListAsync(ct);
                var dataReader = await GetGameVersionsFromReaderAsync(reader, moduleInfosReader).ToListAsync(ct);
                foreach (var grouping in dataReader.GroupBy(x => new { x.Item1.Id, x.Item1.Version }))
                {
                    yield return new()
                    {
                        ModuleInfo = grouping.Select(x => x.Item1).First(),
                        FileId = fileId,
                        Uploaded = uploadedTimestamp,
                        GameVersions = grouping.Select(x => GameVersion.From(x.Item3)).ToArray()
                    };
                }
                continue;
            }

            if (archive.Type == ArchiveType.SevenZip)
                httpStream.SetBufferSize(LargeBufferSize);

            if (archive.Type == ArchiveType.Rar)
                httpStream.SetBufferSize(LargeBufferSize);

            var moduleInfosArchive = await GetModuleInfosFromArchiveAsync(archive, subModuleCount).ToListAsync(ct);
            var dataArchive = await GetGameVersionsFromArchiveAsync(archive, moduleInfosArchive).ToListAsync(ct);
            foreach (var grouping in dataArchive.GroupBy(x => new { x.Item1.Id, x.Item1.Version }))
            {
                yield return new()
                {
                    ModuleInfo = grouping.Select(x => x.Item1).First(),
                    FileId = fileId,
                    Uploaded = uploadedTimestamp,
                    GameVersions = grouping.Select(x => GameVersion.From(x.Item3)).ToArray()
                };
            }
        }
    }

    private static async IAsyncEnumerable<ModuleInfoExtendedWithPath> GetModuleInfosFromReaderAsync(IReader reader, int subModuleCount)
    {
        while (subModuleCount > 0 && reader.MoveToNextEntry())
        {
            if (reader.Entry.IsDirectory) continue;

            if (!reader.Entry.Key.Contains("SubModule.xml", StringComparison.OrdinalIgnoreCase)) continue;

            await using var stream = reader.OpenEntryStream();
            if (GetModuleInfo(stream) is not { } moduleInfo) continue;

            yield return new ModuleInfoExtendedWithPath(moduleInfo, reader.Entry.Key);
            subModuleCount--;
        }
    }

    private static async IAsyncEnumerable<ModuleInfoExtendedWithPath> GetModuleInfosFromArchiveAsync(IArchive archive, int subModuleCount)
    {
        foreach (var entry in archive.Entries)
        {
            if (subModuleCount <= 0) break;

            if (entry.IsDirectory) continue;

            if (!entry.Key.Contains("SubModule.xml", StringComparison.OrdinalIgnoreCase)) continue;

            await using var stream = entry.OpenEntryStream();
            if (GetModuleInfo(stream) is not { } moduleInfo) continue;

            yield return new ModuleInfoExtendedWithPath(moduleInfo, entry.Key);
            subModuleCount--;
        }
    }

    private static async IAsyncEnumerable<(ModuleInfoExtendedWithPath, SubModuleInfoExtended, string)> GetGameVersionsFromReaderAsync(IReader reader, ICollection<ModuleInfoExtendedWithPath> moduleInfos)
    {
        // TODO:
        foreach (var moduleInfo in moduleInfos)
        {
            yield return (moduleInfo, null!, string.Empty);
        }
        yield break;

        var count = moduleInfos.SelectMany(x => x.SubModules).Select(x => x.DLLName).Count();
        while (count > 0 && reader.MoveToNextEntry())
        {
            if (reader.Entry.IsDirectory) continue;

            if (!reader.Entry.Key.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) continue;

            foreach (var moduleInfo in moduleInfos)
            {
                var basePath = Path.GetDirectoryName(moduleInfo.Path);
                if (string.IsNullOrEmpty(basePath)) continue;
                if (!reader.Entry.Key.Contains(basePath)) continue;
                foreach (var subModule in moduleInfo.SubModules)
                {
                    if (!reader.Entry.Key.EndsWith(subModule.DLLName)) continue;

                    await using var stream = reader.OpenEntryStream();
                    var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    var assembly = AssemblyDefinition.FromImage(PEImage.FromDataSource(new StreamDataSource(ms)));
                    foreach (var gameVersion in GetGameVersions(assembly))
                        yield return (moduleInfo, subModule, gameVersion);
                    count--;
                }
            }
        }
    }

    private static async IAsyncEnumerable<(ModuleInfoExtendedWithPath, SubModuleInfoExtended, string)> GetGameVersionsFromArchiveAsync(IArchive archive, ICollection<ModuleInfoExtendedWithPath> moduleInfos)
    {
        // TODO:
        foreach (var moduleInfo in moduleInfos)
        {
            yield return (moduleInfo, null!, string.Empty);
        }
        yield break;

        var count = moduleInfos.SelectMany(x => x.SubModules).Select(x => x.DLLName).Count();
        foreach (var entry in archive.Entries)
        {
            if (count <= 0) break;

            if (entry.IsDirectory) continue;

            if (!entry.Key.Contains(".dll", StringComparison.OrdinalIgnoreCase)) continue;

            foreach (var moduleInfo in moduleInfos)
            {
                var basePath = Path.GetDirectoryName(moduleInfo.Path);
                if (string.IsNullOrEmpty(basePath)) continue;
                if (!entry.Key.Contains(basePath)) continue;
                foreach (var subModule in moduleInfo.SubModules)
                {
                    if (!entry.Key.EndsWith(subModule.DLLName)) continue;

                    await using var stream = entry.OpenEntryStream();
                    var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    var assembly = AssemblyDefinition.FromImage(PEImage.FromDataSource(new StreamDataSource(ms)));
                    foreach (var gameVersion in GetGameVersions(assembly))
                        yield return (moduleInfo, subModule, gameVersion);
                    count--;
                }
            }
        }
    }

    private static IEnumerable<string> GetGameVersions(AssemblyDefinition assemblyDefinition)
    {
        // TODO:
        yield break;
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