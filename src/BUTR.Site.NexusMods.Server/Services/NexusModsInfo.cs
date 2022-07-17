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
using System.Net.Http;
using System.Xml.Serialization;

namespace BUTR.Site.NexusMods.Server.Services
{
    public class NexusModsInfo
    {
        [XmlRoot("Module")]
        private record SubModuleXml
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

        public async IAsyncEnumerable<string> GetModIds(string gameDomain, int modId, string apiKey)
        {
            var fileInfos = await _apiClient.GetModFileInfos(gameDomain, modId, apiKey);
            foreach (var fileInfo in fileInfos?.Files ?? Array.Empty<NexusModsModFilesResponse.File>())
            {
                var downloadLinks = await _apiClient.GetModFileLinks(gameDomain, modId, fileInfo.FileId, apiKey);
                var url = downloadLinks.First().URI;
                await using var httpStream = new HttpRangeStream(new Uri(url), _httpClient);
                using var archive = Path.GetExtension(fileInfo.FileName) switch
                {
                    ".7z" => (IArchive) SevenZipArchive.Open(httpStream),
                    ".rar" => (IArchive) RarArchive.Open(httpStream),
                    ".zip" => (IArchive) ZipArchive.Open(httpStream),
                    _ => null
                };

                var subModule = archive?.Entries.FirstOrDefault(x => x.Key.Contains("SubModule.xml", StringComparison.InvariantCulture));
                if (subModule is null) continue;

                using var reader = new StreamReader(subModule.OpenEntryStream(), leaveOpen: true);
                var serializer = new XmlSerializer(typeof(SubModuleXml));
                if (serializer.Deserialize(reader) is not SubModuleXml result) continue;
                yield return result.Id.Value;
            }
        }
    }
}