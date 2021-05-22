using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUTR.CrashReportViewer.Shared.Models.API
{
    public class PagingResponse<T> where T : class
    {
        public List<T> Items { get; set; }
        public PagingMetadata Metadata { get; set; }
    }
}