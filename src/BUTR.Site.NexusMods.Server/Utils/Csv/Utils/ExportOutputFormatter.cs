using CsvHelper;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Utils.Csv.Utils;

public class ExportOutputFormatter<TCsvFile> : TextOutputFormatter where TCsvFile: ICsvFile
{
    private static string ContentType => "text/csv";

    public ExportOutputFormatter()
    {
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(ContentType));

        SupportedEncodings.Add(Encoding.UTF8);
        SupportedEncodings.Add(Encoding.Unicode);
    }

    protected override bool CanWriteType(Type? type)
    {
        if (typeof(TCsvFile).IsAssignableFrom(type) || typeof(IEnumerable<TCsvFile>).IsAssignableFrom(type))
            return base.CanWriteType(type);
        return false;
    }

    public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
    {
        var (data, type) = context.Object switch
        {
            IEnumerable<TCsvFile> enumerable => (enumerable, enumerable.GetType().GetGenericArguments()[0]),
            TCsvFile entry => (new List<TCsvFile> { entry }, entry.GetType()),
            _ => default
        };
        if (data is null || type is null)
            throw new InvalidOperationException("Invalid csv data type!");
        
        await WriteCsvCollection(context.HttpContext, data, selectedEncoding);
    }

    private static async Task WriteCsvCollection(HttpContext context, IEnumerable<TCsvFile> entries, Encoding selectedEncoding)
    {
        var response = context.Response;

        response.Headers.ContentDisposition = new System.Net.Mime.ContentDisposition
        {
            FileName = TCsvFile.GenerateFilename(),
            Inline = false,
        }.ToString();

        await using var csv = new CsvWriter(new StreamWriter(response.Body, selectedEncoding), CultureInfo.InvariantCulture);
        
        csv.WriteHeader<TCsvFile>();
        await csv.NextRecordAsync();
        await csv.WriteRecordsAsync(entries);
    }
}