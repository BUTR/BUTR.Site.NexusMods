using CsvHelper;

using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Utils.Csv.Utils;

public class ExportOutputFormatter<TCsvFile> : TextOutputFormatter where TCsvFile : ICsvFile
{
    private static string ContentType => "text/csv";

    public ExportOutputFormatter()
    {
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(ContentType));

        SupportedEncodings.Add(Encoding.UTF8);
        SupportedEncodings.Add(Encoding.Unicode);
    }

    public bool CanWriteTypeExposed(Type? type) => CanWriteType(type);
    protected override bool CanWriteType(Type? type)
    {
        if (typeof(TCsvFile).IsAssignableFrom(type) || typeof(IEnumerable<TCsvFile>).IsAssignableFrom(type))
            return base.CanWriteType(type);
        return false;
    }

    public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
    {
        var response = context.HttpContext.Response;

        response.Headers.ContentDisposition = new ContentDisposition
        {
            FileName = TCsvFile.GenerateFilename(),
            Inline = false,
        }.ToString();

        switch (context.Object)
        {
            case TCsvFile entry:
                await WriteCsvEnumerableAsync(response.Body, new List<TCsvFile> { entry }, selectedEncoding);
                break;
            case IEnumerable<TCsvFile> enumerable:
                await WriteCsvEnumerableAsync(response.Body, enumerable, selectedEncoding);
                break;
            case IAsyncEnumerable<TCsvFile> asyncEnumerable:
                await WriteCsvAsyncEnumerableAsync(response.Body, asyncEnumerable, selectedEncoding);
                break;
            default:
                throw new InvalidOperationException("Invalid csv data type!");
        }
    }

    private static async Task WriteCsvEnumerableAsync(Stream stream, IEnumerable<TCsvFile> entries, Encoding selectedEncoding)
    {
        await using var csv = new CsvWriter(new StreamWriter(stream, selectedEncoding), CultureInfo.InvariantCulture, true);

        csv.WriteHeader<TCsvFile>();
        await csv.NextRecordAsync();
        await csv.WriteRecordsAsync(entries);
    }

    private static async Task WriteCsvAsyncEnumerableAsync(Stream stream, IAsyncEnumerable<TCsvFile> entries, Encoding selectedEncoding)
    {
        await using var csv = new CsvWriter(new StreamWriter(stream, selectedEncoding), CultureInfo.InvariantCulture, true);

        csv.WriteHeader<TCsvFile>();
        await csv.NextRecordAsync();
        await csv.WriteRecordsAsync(entries);
    }
}