using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Utils;

using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives.Zip;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Xml.Serialization;

namespace BUTR.Site.NexusMods.Server.Services
{
    public class NexusModsInfo
    {
        [XmlRoot("Module")]
        public record SubModuleXml
        {
            public record IdValue
            {
                [XmlAttribute("value")]
                public string Value { get; init; } = default!;
            }

            [XmlElement("Id")]
            public IdValue Id { get; init; } = default!;
        }

        private readonly HttpClient _httpClient;
        private readonly NexusModsAPIClient _apiClient;

        public NexusModsInfo(HttpClient httpClient, NexusModsAPIClient apiClient)
        {
            _httpClient = httpClient;
            _apiClient = apiClient;
        }

        private static bool ContainsSubModuleFile(IReadOnlyList<NexusModsModFileContentResponse.ContentEntry> entries)
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

        public async IAsyncEnumerable<string> GetModIdsAsync(string gameDomain, int modId, string apiKey)
        {
            var fileInfos = await _apiClient.GetModFileInfosAsync(gameDomain, modId, apiKey);
            foreach (var fileInfo in fileInfos?.Files ?? Array.Empty<NexusModsModFilesResponse.File>())
            {
                try
                {
                    var content = await _httpClient.GetFromJsonAsync<NexusModsModFileContentResponse>(fileInfo.ContentPreviewUrl);
                    if (content is null) continue;
                    if (!ContainsSubModuleFile(content.Children)) continue;
                }
                catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.NotFound)
                {
                    continue;
                }


                var downloadLinks = await _apiClient.GetModFileLinksAsync(gameDomain, modId, fileInfo.Id, apiKey) ?? Array.Empty<NexusModsDownloadLinkResponse>();
                foreach (var downloadLink in downloadLinks)
                {
                    var extension = Path.GetExtension(fileInfo.FileName);
                    var uri = new Uri(downloadLink.Url);

                    await using var httpStream = extension switch
                    {
                        // 7z files do not support Stream seeking because of LZMA and require to download
                        // the whole file section and read-skip the content until we hit the needed position
                        // Because of that, we need a bigger buffer to make fewer requests
                        ".7z" => HttpRangeStream.Create(uri, _httpClient, 1024 * 1024 * 5),
                        ".rar" => HttpRangeStream.Create(uri, _httpClient, 1024 * 4),
                        ".zip" => HttpRangeStream.Create(uri, _httpClient, 1024 * 4),
                        _ => null,
                    };
                    if (httpStream is null) continue;

                    using IArchive? archive = extension switch
                    {
                        ".7z" => SevenZipArchive.Open(httpStream),
                        ".rar" => RarArchive.Open(httpStream),
                        ".zip" => ZipArchive.Open(httpStream),
                        _ => null,
                    };
                    if (archive is null) continue;

                    var subModule = archive.Entries.FirstOrDefault(x => x.Key.Contains("SubModule.xml", StringComparison.OrdinalIgnoreCase));
                    if (subModule is null) continue;

                    string id;
                    try
                    {
                        await using var entryStream = subModule.OpenEntryStream();
                        using var reader = new StreamReader(entryStream);
                        var serializer = new XmlSerializer(typeof(SubModuleXml));
                        if (serializer.Deserialize(reader) is not SubModuleXml result) continue;
                        id = result.Id.Value;
                    }
                    catch (Exception)
                    {
                        id = "ERROR";
                    }

                    yield return id;
                    break;
                }
            }
        }
    }
}