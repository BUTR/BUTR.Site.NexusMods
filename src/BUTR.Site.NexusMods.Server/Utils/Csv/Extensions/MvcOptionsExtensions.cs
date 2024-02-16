using BUTR.Site.NexusMods.Server.Utils.Csv.Utils;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace BUTR.Site.NexusMods.Server.Utils.Csv.Extensions;

[ExcludeFromCodeCoverage]
public static class MvcOptionsExtensions
{
    public static void AddCsvOutputFormatters(this MvcOptions options)
    {
        var types = typeof(Program).Assembly.GetTypes().Where(t => t.IsAssignableTo(typeof(ICsvFile)) && !t.IsAbstract);
        foreach (var type in types)
            options.OutputFormatters.Insert(0, (IOutputFormatter) Activator.CreateInstance(typeof(ExportOutputFormatter<>).MakeGenericType(type))!);
    }
}