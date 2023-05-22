using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Utils.Http.StreamingJson;

public class StreamingJsonActionResult : IActionResult
{
    public StreamingJsonActionResult(IEnumerable<Func<Stream, CancellationToken, Task>> contents, string mime)
    {
        Contents = contents;
        Mime = mime;
    }

    public IEnumerable<Func<Stream, CancellationToken, Task>> Contents { get; }
    public string Mime { get; }

    public Task ExecuteResultAsync(ActionContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var executor = context
            .HttpContext
            .RequestServices
            .GetRequiredService<IActionResultExecutor<StreamingJsonActionResult>>();

        return executor.ExecuteAsync(context, this);
    }
}