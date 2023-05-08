using HtmlAgilityPack;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BUTR.Site.NexusMods.Shared.Helpers;

public record CrashReport
{
    public required Guid Id { get; init; }
    public required int Version { get; init; }
    public required string GameVersion { get; init; }
    public required string Exception { get; init; }
    public required ImmutableArray<Module> Modules { get; init; }
    public required ImmutableArray<InvolvedModule> InvolvedModules { get; init; }
    public required ImmutableArray<EnhancedStacktraceFrame> EnhancedStacktrace { get; init; }
    public required string Id2 { get; init; }

    public required string? LauncherType { get; init; }
    public required string? LauncherVersion { get; init; }

    public required string? Runtime { get; init; }

    public required string? BUTRLoaderVersion { get; init; }

    public required string? BLSEVersion { get; init; }
    public required string? LauncherExVersion { get; init; }
}

public record EnhancedStacktraceFrame
{
    public required string Name { get; init; }
    public required int ILOffset { get; init; }
    public required ImmutableArray<EnhancedStacktraceFrameMethod> Methods { get; init; }
}
public record EnhancedStacktraceFrameMethod
{
    public required string Module { get; init; }
    public required string MethodFullName { get; init; }
    public required string Method { get; init; }
    public required ImmutableArray<string> MethodParameters { get; init; }
}

public record ModuleDependencyMetadatas
{
    public required string Type { get; init; }
    public required string ModuleId { get; init; }
}

public record ModuleSubModule
{
    public required string Name { get; init; }
    public required string DLLName { get; init; }
    public required string SubModuleClassType { get; init; }
    public required ImmutableArray<KeyValuePair<string, string>> Tags { get; init; }
}

public sealed record Module
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Alias { get; init; }
    public required string Version { get; init; }
    public required string IsExternal { get; init; }
    public required string IsOfficial { get; init; }
    public required string IsSingleplayer { get; init; }
    public required string IsMultiplayer { get; init; }
    public required string Url { get; init; }
    public required ImmutableList<ModuleDependencyMetadatas> DependencyMetadatas { get; init; }
    public required ImmutableList<ModuleSubModule> SubModules { get; init; }
}

public record InvolvedModule
{
    public required string Id { get; init; }
    public required string Stacktrace { get; init; }
}

public static class CrashReportParser
{
    public static async Task<CrashReport> ParseUrl(HttpClient client, string id)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{id}.html");
        var response = await client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        return Parse(id, content);
    }

    private delegate bool MatchSpan(ReadOnlySpan<char> span);
    private static IEnumerable<string> GetAllOpenTags(ReadOnlySpan<char> content, MatchSpan matcher)
    {
        var toReplace = new List<string>();
        var span = content;
        while (span.IndexOf('<') is var idxOpen and not -1 && span.Slice(idxOpen).IndexOf('>') is var idxClose and not -1)
        {
            var tag = span.Slice(idxOpen, idxClose + 1);
            span = span.Slice(idxOpen + idxClose + 1);
            if (tag.Length < 2 || tag[1] == '/' || tag[^2] == '/') continue;
            if (matcher(tag)) toReplace.Add(tag.ToString());
        }
        return toReplace;
    }

    private static ImmutableArray<EnhancedStacktraceFrame> GetEnhancedStacktrace(ReadOnlySpan<char> rawContent, int version, HtmlNode node)
    {
        if (version < 1000)
        {
            const string enhancedStacktraceStartDelimiter = "<div id='enhanced-stacktrace' class='headers-container'>";
            const string enhancedStacktraceEndDelimiter = "</div>";
            var enhancedStacktraceStartIdx= rawContent.IndexOf(enhancedStacktraceStartDelimiter);
            var enhancedStacktraceEndIdx= rawContent.Slice(enhancedStacktraceStartIdx).IndexOf(enhancedStacktraceEndDelimiter) - enhancedStacktraceEndDelimiter.Length;
            var enhancedStacktraceRaw = rawContent.Slice(enhancedStacktraceStartIdx, enhancedStacktraceEndIdx).ToString();
            var toEscape = GetAllOpenTags(enhancedStacktraceRaw, span => !span.SequenceEqual(enhancedStacktraceStartDelimiter) && span is not "<ul>" and not "<li>" and not "<br>").ToArray();
            enhancedStacktraceRaw = toEscape.Aggregate(enhancedStacktraceRaw, (current, s) => current.Replace(s, s.Replace("<", "&lt;").Replace(">", "&gt;")));
            //var openTags = GetAllOpenTags(enhancedStacktraceRaw, span => !span.SequenceEqual("<ul>")  && !span.SequenceEqual("<li>") && !span.SequenceEqual("<br>")).ToArray();
            var enhancedStacktraceDoc = new HtmlDocument();
            enhancedStacktraceDoc.LoadHtml(enhancedStacktraceRaw);
            node = enhancedStacktraceDoc.DocumentNode;
        }
        
        return node.SelectSingleNode("descendant::div[@id=\"enhanced-stacktrace\"]/ul")?.ChildNodes.Where(cn => cn.Name == "li").Select(ParseEnhancedStacktrace).ToImmutableArray() ?? ImmutableArray<EnhancedStacktraceFrame>.Empty;
    }
    public static CrashReport Parse(string id2, string content)
    {
        var html = new HtmlDocument();
        html.LoadHtml(content.Replace("<filename unknown>", "NULL"));
        var document = html.DocumentNode;

        var id = document.SelectSingleNode("descendant::report")?.Attributes?["id"]?.Value ?? string.Empty;
        var versionStr = document.SelectSingleNode("descendant::report")?.Attributes?["version"]?.Value;
        var version = int.TryParse(versionStr, out var v) ? v : 1;
        var gameVersion = document.SelectSingleNode("descendant::game")?.Attributes?["version"]?.Value ?? string.Empty;
        var exception = document.SelectSingleNode("descendant::div[@id=\"exception\"]")?.InnerText ?? string.Empty;
        var installedModules = document.SelectSingleNode("descendant::div[@id=\"installed-modules\"]/ul")?.ChildNodes.Where(cn => cn.Name == "li").Select(ParseModule).ToImmutableArray() ?? ImmutableArray<Module>.Empty;
        var involvedModules = document.SelectSingleNode("descendant::div[@id=\"involved-modules\"]/ul")?.ChildNodes.Where(cn => cn.Name == "li").Select(ParseInvolvedModule).ToImmutableArray() ?? ImmutableArray<InvolvedModule>.Empty;
        var enhancedStacktrace = GetEnhancedStacktrace(content.AsSpan(), version, document);

        //var assemblies = document.SelectSingleNode("descendant::div[@id=\"assemblies\"]/ul").ChildNodes.Where(cn => cn.Name == "li").ToList();
        //var harmonyPatches = document.SelectSingleNode("descendant::div[@id=\"harmony-patches\"]/ul").ChildNodes.Where(cn => cn.Name == "li").ToList();
        var launcherType = document.SelectSingleNode("descendant::launcher")?.Attributes?["type"]?.Value ?? string.Empty;
        var launcherVersion = document.SelectSingleNode("descendant::launcher")?.Attributes?["version"]?.Value ?? string.Empty;
        var runtime = document.SelectSingleNode("descendant::runtime")?.Attributes?["value"]?.Value ?? string.Empty;
        var butrloaderVersion = document.SelectSingleNode("descendant::butrloader")?.Attributes?["version"]?.Value ?? string.Empty;
        var blseVersion = document.SelectSingleNode("descendant::blse")?.Attributes?["version"]?.Value ?? string.Empty;
        var launcherexVersion = document.SelectSingleNode("descendant::launcherex")?.Attributes?["version"]?.Value ?? string.Empty;
        return new CrashReport
        {
            Id = Guid.TryParse(id, out var val) ? val : Guid.Empty,
            Version = version,
            GameVersion = gameVersion,
            Exception = exception,
            Modules = installedModules,
            InvolvedModules = involvedModules,
            EnhancedStacktrace = enhancedStacktrace,
            Id2 = id2,
            LauncherType = launcherType,
            LauncherVersion = launcherVersion,
            Runtime = runtime,
            BUTRLoaderVersion = butrloaderVersion,
            BLSEVersion = blseVersion,
            LauncherExVersion = launcherexVersion,
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
            IsExternal = GetField(lines, "External"),
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

    private static EnhancedStacktraceFrame ParseEnhancedStacktrace(HtmlNode node)
    {
        var frameLine = node.ChildNodes.FirstOrDefault()?.InnerText.Trim().Replace("\r\n", string.Empty) ?? string.Empty;
        var name = frameLine;
        var ilOffset = int.TryParse(frameLine.Split("(IL Offset: ").Skip(1).FirstOrDefault()?.Replace(")", string.Empty).Trim(), out var ilOffsetVal) ? ilOffsetVal : -1;

        var methodsBuilder = ImmutableArray.CreateBuilder<EnhancedStacktraceFrameMethod>();
        foreach (var childNode in node.ChildNodes.FirstOrDefault(x => x.Name == "ul")?.ChildNodes ?? Enumerable.Empty<HtmlNode>())
        {
            var lines = childNode.InnerHtml.Replace("&lt;", "<").Replace("&gt;", ">").Trim().Split("<br>", StringSplitOptions.RemoveEmptyEntries);
            var module = lines?.Length > 0 ? lines[0].Substring(8) : string.Empty;
            var methodFullName = lines?.Length > 1 ? lines[1].Substring(8).Replace("::", ".") : string.Empty;
            var idx1 = methodFullName.IndexOf("(", StringComparison.Ordinal);
            var idx2 = idx1 != -1 ? methodFullName.Substring(0, idx1).LastIndexOf(" ", StringComparison.Ordinal) : -1;
            var method = idx2 != -1 ? methodFullName.Substring(idx2 + 1) : string.Empty;
            var methodSplit = method.Split("(");
            var parameters = methodSplit.Length > 1
                ? methodSplit[1].Trim(')').Split(" ", StringSplitOptions.RemoveEmptyEntries)
                    .Where((_, i) => i % 2 == 0)
                    .Select(x => x.Trim(','))
                    .ToImmutableArray()
                : ImmutableArray<string>.Empty;
            methodsBuilder.Add(new EnhancedStacktraceFrameMethod
            {
                Module = module,
                MethodFullName = methodFullName,
                Method = methodSplit[0].Replace("::", "."),
                MethodParameters = parameters
            });
        }

        return new EnhancedStacktraceFrame
        {
            Name = name,
            ILOffset = ilOffset,
            Methods = methodsBuilder.ToImmutable(),
        };
    }
}