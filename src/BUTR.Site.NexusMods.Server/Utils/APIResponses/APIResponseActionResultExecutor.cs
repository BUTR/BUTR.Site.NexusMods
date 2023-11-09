using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

using System;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Utils.APIResponses;

internal abstract class APIResponseActionResultExecutor
{
    protected static readonly string DefaultContentType = new MediaTypeHeaderValue("application/json")
    {
        Encoding = Encoding.UTF8
    }.ToString();
}

internal sealed partial class APIResponseActionResultExecutor<TValue> : APIResponseActionResultExecutor, IActionResultExecutor<APIResponseActionResultModel<TValue>>
{
    private readonly ILogger _logger;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public APIResponseActionResultExecutor(ILogger<APIResponseActionResultExecutor<TValue>> logger, IOptions<JsonSerializerOptions> jsonSerializerOptions)
    {
        _logger = logger;
        _jsonSerializerOptions = jsonSerializerOptions.Value;
    }

    public async Task ExecuteAsync(ActionContext context, APIResponseActionResultModel<TValue> result)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(result);

        var response = context.HttpContext.Response;

        ResponseContentTypeHelper.ResolveContentTypeAndEncoding(
            /*result.ContentType,*/
            response.ContentType,
            (DefaultContentType, Encoding.UTF8),
            MediaType.GetEncoding,
            out var resolvedContentType,
            out var resolvedContentTypeEncoding);

        response.ContentType = resolvedContentType;

        if (result.Error?.Status != null)
        {
            response.StatusCode = result.Error.Status.Value;
        }

        Log.JsonResultExecuting(_logger, result);

        var value = result;
        var jsonSerializerOptions = _jsonSerializerOptions;

        // Keep this code in sync with SystemTextJsonOutputFormatter
        var responseStream = response.Body;
        if (resolvedContentTypeEncoding.CodePage == Encoding.UTF8.CodePage)
        {
            try
            {
                await JsonSerializer.SerializeAsync(responseStream, value, jsonSerializerOptions, context.HttpContext.RequestAborted);
                await responseStream.FlushAsync(context.HttpContext.RequestAborted);
            }
            catch (OperationCanceledException) when (context.HttpContext.RequestAborted.IsCancellationRequested) { }
        }
        else
        {
            // JsonSerializer only emits UTF8 encoded output, but we need to write the response in the encoding specified by
            // selectedEncoding
            var transcodingStream = Encoding.CreateTranscodingStream(response.Body, resolvedContentTypeEncoding, Encoding.UTF8, leaveOpen: true);

            ExceptionDispatchInfo? exceptionDispatchInfo = null;
            try
            {
                await JsonSerializer.SerializeAsync(transcodingStream, value, jsonSerializerOptions, context.HttpContext.RequestAborted);
                await transcodingStream.FlushAsync(context.HttpContext.RequestAborted);
            }
            catch (OperationCanceledException) when (context.HttpContext.RequestAborted.IsCancellationRequested)
            { }
            catch (Exception ex)
            {
                // TranscodingStream may write to the inner stream as part of it's disposal.
                // We do not want this exception "ex" to be eclipsed by any exception encountered during the write. We will stash it and
                // explicitly rethrow it during the finally block.
                exceptionDispatchInfo = ExceptionDispatchInfo.Capture(ex);
            }
            finally
            {
                try
                {
                    await transcodingStream.DisposeAsync();
                }
                catch when (exceptionDispatchInfo != null)
                {
                }

                exceptionDispatchInfo?.Throw();
            }
        }
    }

    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Information, "Executing JsonResult, writing value of type '{Type}'.", EventName = "JsonResultExecuting", SkipEnabledCheck = true)]
        private static partial void JsonResultExecuting(ILogger logger, string? type);

        public static void JsonResultExecuting(ILogger logger, object? value)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                var type = value == null ? "null" : value.GetType().FullName;
                JsonResultExecuting(logger, type);
            }
        }
    }
}