using BUTR.Site.NexusMods.Server.Utils.Csv;
using BUTR.Site.NexusMods.Server.Utils.Csv.Utils;

using CsvHelper.Configuration.Attributes;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;

using System.Text;

namespace BUTR.Site.NexusMods.Server.Tests.Csv;

public class ExportOutputFormatterTests
{
    [Delimiter(",")]
    private class MockCsvFile : ICsvFile
    {
        [Name("Test"), Index(0)]
        public string Test { get; init; }

        public static string GenerateFilename() => "mock.csv";
    }

    [Fact]
    public void CanWriteType_ReturnsTrue_ForAssignableTypes()
    {
        var formatter = new ExportOutputFormatter<MockCsvFile>();
        Assert.True(formatter.CanWriteTypeExposed(typeof(MockCsvFile)));
        Assert.True(formatter.CanWriteTypeExposed(typeof(List<MockCsvFile>)));
    }

    [Fact]
    public void CanWriteType_ReturnsFalse_ForNonAssignableTypes()
    {
        var formatter = new ExportOutputFormatter<MockCsvFile>();
        Assert.False(formatter.CanWriteTypeExposed(typeof(string)));
    }

    [Fact]
    public async Task WriteResponseBodyAsync_WritesSingleEntry_ToResponseBodyAsync()
    {
        var formatter = new ExportOutputFormatter<MockCsvFile>();
        var context = new OutputFormatterWriteContext(
            new DefaultHttpContext(),
            (stream, encoding) => new StreamWriter(stream, encoding),
            typeof(MockCsvFile),
            new MockCsvFile());

        await formatter.WriteResponseBodyAsync(context, Encoding.UTF8);

        // Assert that the response body contains the expected CSV data
    }

    [Fact]
    public async Task WriteResponseBodyAsync_WritesEnumerable_ToResponseBodyAsync()
    {
        var formatter = new ExportOutputFormatter<MockCsvFile>();
        var context = new OutputFormatterWriteContext(
            new DefaultHttpContext(),
            (stream, encoding) => new StreamWriter(stream, encoding),
            typeof(IEnumerable<MockCsvFile>),
            new List<MockCsvFile> { new MockCsvFile(), new MockCsvFile() });

        await formatter.WriteResponseBodyAsync(context, Encoding.UTF8);

        // Assert that the response body contains the expected CSV data
    }

    [Fact]
    public async Task WriteResponseBodyAsync_ThrowsException_ForInvalidObjectTypeAsync()
    {
        var formatter = new ExportOutputFormatter<MockCsvFile>();
        var context = new OutputFormatterWriteContext(
            new DefaultHttpContext(),
            (stream, encoding) => new StreamWriter(stream, encoding),
            typeof(string),
            "invalid object type");

        await Assert.ThrowsAsync<InvalidOperationException>(() => formatter.WriteResponseBodyAsync(context, Encoding.UTF8));
    }
}