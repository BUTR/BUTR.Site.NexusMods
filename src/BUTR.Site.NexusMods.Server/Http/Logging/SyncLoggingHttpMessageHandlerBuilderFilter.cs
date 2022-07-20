using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;

namespace BUTR.Site.NexusMods.Server.Http.Logging
{
    internal sealed class SyncLoggingHttpMessageHandlerBuilderFilter : IHttpMessageHandlerBuilderFilter
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IOptionsMonitor<HttpClientFactoryOptions> _optionsMonitor;

        public SyncLoggingHttpMessageHandlerBuilderFilter(ILoggerFactory loggerFactory, IOptionsMonitor<HttpClientFactoryOptions> optionsMonitor)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
        }

        public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
        {
            return (builder) =>
            {
                // Run other configuration first, we want to decorate.
                next(builder);

                var loggerName = !string.IsNullOrEmpty(builder.Name) ? builder.Name : "Default";

                // We want all of our logging message to show up as-if they are coming from HttpClient,
                // but also to include the name of the client for more fine-grained control.
                var outerLogger = _loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{loggerName}.LogicalSyncHandler");
                var innerLogger = _loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{loggerName}.ClientSyncHandler");

                var options = _optionsMonitor.Get(builder.Name);

                // The 'scope' handler goes first so it can surround everything.
                builder.AdditionalHandlers.Insert(0, new SyncLoggingScopeHttpMessageHandler(outerLogger, options));

                // We want this handler to be last so we can log details about the request after
                // service discovery and security happen.
                builder.AdditionalHandlers.Add(new SyncLoggingHttpMessageHandler(innerLogger, options));
            };
        }
    }
}