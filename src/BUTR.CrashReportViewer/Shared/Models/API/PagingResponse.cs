using System.Collections.Generic;

namespace BUTR.CrashReportViewer.Shared.Models.API
{
    public class PagingResponse<T> where T : class
    {
        public List<T> Items { get; set; } = new();
        public PagingMetadata Metadata { get; set; } = new();
    }
}