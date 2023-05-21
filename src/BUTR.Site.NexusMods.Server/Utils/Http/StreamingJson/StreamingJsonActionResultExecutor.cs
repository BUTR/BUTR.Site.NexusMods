using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Net.Http.Headers;

using System;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Utils.Http.StreamingJson;

public class StreamingJsonActionResultExecutor : IActionResultExecutor<StreamingJsonActionResult>
{
    private static readonly byte[] CrLf = "\r\n"u8.ToArray();

    public async Task ExecuteAsync(ActionContext context, StreamingJsonActionResult result)
    {
        var response = context.HttpContext.Response;

        var responseHeaders = response.GetTypedHeaders();
        responseHeaders.ContentType = new MediaTypeHeaderValue(result.Mime);

        var responseStream = response.Body;

        try
        {
            foreach (var contentStreamFunc in result.Contents)
            {
                await contentStreamFunc(responseStream, context.HttpContext.RequestAborted);
                await responseStream.WriteAsync(CrLf);
            }
        }
        catch (OperationCanceledException)
        {
            // Don't throw this exception, it's most likely caused by the client disconnecting.
            // However, if it was cancelled for any other reason we need to prevent empty responses.
            context.HttpContext.Abort();
        }
    }
}