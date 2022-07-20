using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;

namespace BUTR.Site.NexusMods.Server
{
    /// <summary>
    /// Handles logging of the lifecycle for an HTTP request within a log scope.
    /// </summary>
    public class SyncLoggingScopeHttpMessageHandler : DelegatingHandler
    {
        private readonly ILogger _logger;
        private readonly HttpClientFactoryOptions? _options;

        private static readonly Func<string, bool> _shouldNotRedactHeaderValue = (header) => false;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncLoggingScopeHttpMessageHandler"/> class with a specified logger.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to log to.</param>
        /// <exception cref="ArgumentNullException"><paramref name="logger"/> is <see langword="null"/>.</exception>
        public SyncLoggingScopeHttpMessageHandler(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncLoggingScopeHttpMessageHandler"/> class with a specified logger and options.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to log to.</param>
        /// <param name="options">The <see cref="HttpClientFactoryOptions"/> used to configure the <see cref="SyncLoggingScopeHttpMessageHandler"/> instance.</param>
        /// <exception cref="ArgumentNullException"><paramref name="logger"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
        public SyncLoggingScopeHttpMessageHandler(ILogger logger, HttpClientFactoryOptions options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Core(request, cancellationToken);

            HttpResponseMessage Core(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var stopwatch = Stopwatch.StartNew();

                var shouldRedactHeaderValue = _options?.ShouldRedactHeaderValue ?? _shouldNotRedactHeaderValue;

                using (Log.BeginRequestPipelineScope(_logger, request))
                {
                    Log.RequestPipelineStart(_logger, request, shouldRedactHeaderValue);
                    var response = base.Send(request, cancellationToken);
                    Log.RequestPipelineEnd(_logger, response, stopwatch.Elapsed, shouldRedactHeaderValue);

                    return response;
                }
            }
        }

        // Used in tests
        internal static class Log
        {
            public static class EventIds
            {
                public static readonly EventId PipelineStart = new(100, "RequestPipelineStart");
                public static readonly EventId PipelineEnd = new(101, "RequestPipelineEnd");

                public static readonly EventId RequestHeader = new(102, "RequestPipelineRequestHeader");
                public static readonly EventId ResponseHeader = new(103, "RequestPipelineResponseHeader");
            }

            private static readonly Func<ILogger, HttpMethod, string?, IDisposable?> _beginRequestPipelineScope = LoggerMessage.DefineScope<HttpMethod, string?>("HTTP {HttpMethod} {Uri}");

            private static readonly Action<ILogger, HttpMethod, string?, Exception?> _requestPipelineStart = LoggerMessage.Define<HttpMethod, string?>(
                LogLevel.Information,
                EventIds.PipelineStart,
                "Start processing HTTP request {HttpMethod} {Uri}");

            private static readonly Action<ILogger, double, int, Exception?> _requestPipelineEnd = LoggerMessage.Define<double, int>(
                LogLevel.Information,
                EventIds.PipelineEnd,
                "End processing HTTP request after {ElapsedMilliseconds}ms - {StatusCode}");

            public static IDisposable? BeginRequestPipelineScope(ILogger logger, HttpRequestMessage request)
            {
                return _beginRequestPipelineScope(logger, request.Method, GetUriString(request.RequestUri));
            }

            public static void RequestPipelineStart(ILogger logger, HttpRequestMessage request, Func<string, bool> shouldRedactHeaderValue)
            {
                _requestPipelineStart(logger, request.Method, GetUriString(request.RequestUri), null);

                if (logger.IsEnabled(LogLevel.Trace))
                {
                    logger.Log(
                        LogLevel.Trace,
                        EventIds.RequestHeader,
                        new HttpHeadersLogValue(HttpHeadersLogValue.Kind.Request, request.Headers, request.Content?.Headers, shouldRedactHeaderValue),
                        null,
                        (state, ex) => state.ToString());
                }
            }

            public static void RequestPipelineEnd(ILogger logger, HttpResponseMessage response, TimeSpan duration, Func<string, bool> shouldRedactHeaderValue)
            {
                _requestPipelineEnd(logger, duration.TotalMilliseconds, (int) response.StatusCode, null);

                if (logger.IsEnabled(LogLevel.Trace))
                {
                    logger.Log(
                        LogLevel.Trace,
                        EventIds.ResponseHeader,
                        new HttpHeadersLogValue(HttpHeadersLogValue.Kind.Response, response.Headers, response.Content?.Headers, shouldRedactHeaderValue),
                        null,
                        (state, ex) => state.ToString());
                }
            }

            private static string? GetUriString(Uri? requestUri)
            {
                return requestUri?.IsAbsoluteUri == true
                    ? requestUri.AbsoluteUri
                    : requestUri?.ToString();
            }
        }
    }
}