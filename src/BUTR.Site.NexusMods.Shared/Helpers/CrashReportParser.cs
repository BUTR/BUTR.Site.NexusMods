using HtmlAgilityPack;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Shared.Helpers
{
    public record CrashRecord
    {
        public Guid Id { get; init; }
        public string GameVersion { get; init; }
        public string Exception { get; init; }
        public ImmutableArray<Module> Modules { get; init; }
        public ImmutableArray<InvolvedModule> InvolvedModules { get; init; }
        public string Id2 { get; init; }
    }

    public record ModuleDependencyMetadatas
    {
        public string Type { get; init; }
        public string ModuleId { get; init; }
    }

    public record ModuleSubModule
    {
        public string Name { get; init; }
        public string DLLName { get; init; }
        public string SubModuleClassType { get; init; }
        public ImmutableArray<KeyValuePair<string, string>> Tags { get; init; }
    }

    public sealed record Module
    {
        public string Id { get; init; }
        public string Name { get; init; }
        public string Alias { get; init; }
        public string Version { get; init; }
        public string IsOfficial { get; init; }
        public string IsSingleplayer { get; init; }
        public string IsMultiplayer { get; init; }
        public string Url { get; init; }
        public ImmutableList<ModuleDependencyMetadatas> DependencyMetadatas { get; init; }
        public ImmutableList<ModuleSubModule> SubModules { get; init; }
    }

    public record InvolvedModule
    {
        public string Id { get; init; }
        public string Stacktrace { get; init; }
    }

    public static class CrashReportParser
    {
        public static async Task<CrashRecord> ParseUrl(HttpClient client, string id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{id}.html");
            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            return Parse(id, content);
        }
        public static CrashRecord Parse(string id2, string content)
        {
            var html = new HtmlDocument();
            html.LoadHtml(content);
            var document = html.DocumentNode;

            var id = document.SelectSingleNode("descendant::report")?.Attributes?["id"]?.Value ?? string.Empty;
            var versionStr = document.SelectSingleNode("descendant::report")?.Attributes?["version"]?.Value;
            var version = int.TryParse(versionStr, out var v) ? v : 1;
            var gameVersion = document.SelectSingleNode("descendant::game")?.Attributes?["version"]?.Value ?? string.Empty;
            var exception = document.SelectSingleNode("descendant::div[@id=\"exception\"]")?.InnerText ?? string.Empty;
            var installedModules = document.SelectSingleNode("descendant::div[@id=\"installed-modules\"]/ul")?.ChildNodes.Where(cn => cn.Name == "li").Select(ParseModule).ToImmutableArray() ?? ImmutableArray<Module>.Empty;
            var involvedModules = document.SelectSingleNode("descendant::div[@id=\"involved-modules\"]/ul")?.ChildNodes.Where(cn => cn.Name == "li").Select(ParseInvolvedModule).ToImmutableArray() ?? ImmutableArray<InvolvedModule>.Empty;
            //var assemblies = document.SelectSingleNode("descendant::div[@id=\"assemblies\"]/ul").ChildNodes.Where(cn => cn.Name == "li").ToList();
            //var harmonyPatches = document.SelectSingleNode("descendant::div[@id=\"harmony-patches\"]/ul").ChildNodes.Where(cn => cn.Name == "li").ToList();
            return new CrashRecord
            {
                Id = Guid.TryParse(id, out var val) ? val : Guid.Empty,
                GameVersion = gameVersion,
                Exception = exception,
                Modules = installedModules,
                InvolvedModules = involvedModules,
                Id2 = id2,
            };
        }

        private static Module ParseModule(HtmlNode node)
        {
            static string GetField(IEnumerable<string> lines, string field) => lines
                .FirstOrDefault(l => l.StartsWith($"{field}:"))?.Split($"{field}:").Skip(1).FirstOrDefault()?.Trim() ?? string.Empty;

            static ImmutableList<string> GetRange(IEnumerable<string> lines, string bField, IEnumerable<string> eFields) => lines
                .SkipWhile(l => !l.StartsWith($"{bField}:")).Skip(1)
                .TakeWhile(l => eFields.All(f => !l.StartsWith($"{f}:")))
                .ToImmutableList();

            static ImmutableList<ModuleDependencyMetadatas> GetModuleDependencyMetadatas(ImmutableList<string> lines) => lines
                .Select(sml => new ModuleDependencyMetadatas
                {
                    Type = sml.StartsWith("Load Before") ? "Load Before" : sml.StartsWith("Load After") ? "Load After" : "ERROR",
                    ModuleId = sml.Replace("Load Before", "").Replace("Load After", "").Trim()
                })
                .ToImmutableList();

            static ImmutableList<ModuleSubModule> GetModuleSubModules(ImmutableList<string> lines) => lines
                .Select((item, index) => new { Item = item, Index = index })
                .Where(o => !o.Item.Contains(':') && !o.Item.Contains(".dll"))
                .Select(o => lines.Skip(o.Index + 1).TakeWhile(l => l.Contains(':') || l.Contains(".dll")).ToImmutableList())
                .Select(sml => new ModuleSubModule
                {
                    Name = sml.FirstOrDefault(l => l.StartsWith("Name:"))?.Split("Name:").Skip(1).FirstOrDefault()?.Trim() ?? string.Empty,
                    DLLName = sml.FirstOrDefault(l => l.StartsWith("DLLName:"))?.Split("DLLName:").Skip(1).FirstOrDefault()?.Trim() ?? string.Empty,
                    SubModuleClassType = sml.FirstOrDefault(l => l.StartsWith("SubModuleClassType:"))?.Split("SubModuleClassType:").Skip(1).FirstOrDefault()?.Trim() ?? string.Empty,
                    Tags = sml.SkipWhile(l => !l.StartsWith("Tags:")).Skip(1).TakeWhile(l => !l.StartsWith("Assemblies:")).Select(l =>
                    {
                        var split = l.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        return new KeyValuePair<string, string>(split[0], split[1]);
                    }).ToImmutableArray()
                })
                .ToImmutableList();

            var lines = node.InnerText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToImmutableList();
            return new Module
            {
                Id = GetField(lines, "Id"),
                Name = GetField(lines, "Name"),
                Alias = GetField(lines, "Alias"),
                Version = GetField(lines, "Version"),
                IsOfficial = GetField(lines, "Official"),
                IsSingleplayer = GetField(lines, "Singleplayer"),
                IsMultiplayer = GetField(lines, "Multiplayer"),
                Url = GetField(lines, "Url"),
                DependencyMetadatas = GetModuleDependencyMetadatas(GetRange(lines, "Dependency Metadatas", new[] { "SubModules", "Additional Assemblies", "Url" })),
                SubModules = GetModuleSubModules(GetRange(lines, "SubModules", new[] { "Additional Assemblies" }))
            };
        }

        private static InvolvedModule ParseInvolvedModule(HtmlNode node)
        {
            var id = node.ChildNodes.FirstOrDefault(x => x.Name == "a")?.InnerText.Trim() ?? string.Empty;
            var lines = string.Join(Environment.NewLine, node.ChildNodes.FirstOrDefault(x => x.Name == "ul")?.ChildNodes.Select(x =>
            {
                x.InnerHtml = x.InnerHtml.Replace("<br>", Environment.NewLine);
                return x.InnerText;
            }) ?? Enumerable.Empty<string>());

            return new InvolvedModule
            {
                Id = id,
                Stacktrace = lines
            };
        }
    }
}