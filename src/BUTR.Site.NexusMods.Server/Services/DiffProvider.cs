using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Utils;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;

namespace BUTR.Site.NexusMods.Server.Services;

public sealed class DiffProvider
{
    public IEnumerable<string> List(string basePath)
    {
        return Directory.GetDirectories(basePath).Select(x => x.Replace(basePath, string.Empty));
    }

    public IEnumerable<string> TreeFlat(string basePath, string entry)
    {
        if (Path.GetInvalidPathChars().Any(entry.Contains)) return Enumerable.Empty<string>();

        var finalPath = Path.GetFullPath(Path.Combine(basePath, entry));
        if (!finalPath.StartsWith(basePath)) return Enumerable.Empty<string>();

        return new DirectoryInfo(finalPath).GetFiles("*.diff", SearchOption.AllDirectories)
            .Where(x => x.Length > 0)
            .Select(x => x.FullName.Replace(basePath, string.Empty));
    }

    public IEnumerable<string> Get(string basePath, string path, CancellationToken ct)
    {
        if (Path.GetInvalidPathChars().Any(path.Contains)) return Enumerable.Empty<string>();

        var finalPath = Path.GetFullPath(Path.Combine(basePath, path));
        if (!finalPath.StartsWith(basePath)) return Enumerable.Empty<string>();

        return GetDiffs(finalPath, ct).Where(x => !string.IsNullOrEmpty(x));
    }

    public IEnumerable<string> Search(string basePath, TextSearchFiltering[] filters, CancellationToken ct)
    {
        var directories = new DirectoryInfo(basePath).GetDirectories();
        var files = directories.SelectMany(x => x.GetFiles("*.diff", SearchOption.AllDirectories).Where(y => y.Length > 0));

        var predicate = CreateExpression(filters);
        return files.SelectMany(x => GetDiffs(x.FullName, ct)).AsQueryable().Where(x => !string.IsNullOrEmpty(x)).Where(predicate);
    }

    private static bool TryOpenFile(string path, [NotNullWhen(true)] out FileStream? stream)
    {
        try
        {
            stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return true;
        }
        catch (Exception)
        {
            stream = null;
            return false;
        }
    }

    private static IEnumerable<string> GetDiffs(string path, CancellationToken ct)
    {
        var builder = new StringBuilder();

        if (!TryOpenFile(path, out var fs)) yield break;

        using var _ = fs;
        using var reader = new StreamReader(fs);
        while (!reader.EndOfStream && !ct.IsCancellationRequested)
        {
            var line = reader.ReadLine();
            if (line is null) break;

            if (line.StartsWith("diff --git", StringComparison.Ordinal))
            {
                yield return builder.ToString();
                builder.Clear();
            }

            builder.AppendLine(line);
        }

        if (builder.Length > 0)
        {
            yield return builder.ToString();
            builder.Clear();
        }
    }

    private static Expression<Func<string, bool>> CreateExpression(IEnumerable<TextSearchFiltering> filters)
    {
        var predicate = PredicateBuilder.True<string>();
        foreach (var filter in filters)
        {
            Expression<Func<string, bool>> innerPredicate = filter.Type.HasFlag(TextSearchFilteringType.Not)
                ? (x => !x.Contains(filter.Value))
                : (x => x.Contains(filter.Value));

            if (filter.Type.HasFlag(TextSearchFilteringType.And))
                predicate = predicate.And(innerPredicate);

            if (filter.Type.HasFlag(TextSearchFilteringType.Or))
                predicate = predicate.Or(innerPredicate);
        }
        return predicate;
    }
}