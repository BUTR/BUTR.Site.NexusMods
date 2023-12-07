using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Utils.Http.StreamingMultipartResults;

public sealed class StreamingMultipartResult : IActionResult
{
    public IEnumerable<Func<Stream, CancellationToken, Task>> Contents { get; }
    public string Mime { get; }
    
    public StreamingMultipartResult(IEnumerable<Func<Stream, CancellationToken, Task>> contents, string mime)
    {
        Contents = contents;
        Mime = mime;
    }

    public Task ExecuteResultAsync(ActionContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var executor = context.HttpContext.RequestServices.GetRequiredService<IActionResultExecutor<StreamingMultipartResult>>();
        return executor.ExecuteAsync(context, this);
    }
}