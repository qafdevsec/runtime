// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Extensions.Http.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Http
{
    // Internal so we can change the requirements without breaking changes.
    internal sealed class LoggingHttpMessageHandlerBuilderFilter : IHttpMessageHandlerBuilderFilter
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IOptionsMonitor<HttpClientFactoryOptions> _optionsMonitor;

        public LoggingHttpMessageHandlerBuilderFilter(ILoggerFactory loggerFactory, IOptionsMonitor<HttpClientFactoryOptions> optionsMonitor)
        {
            ThrowHelper.ThrowIfNull(loggerFactory);
            ThrowHelper.ThrowIfNull(optionsMonitor);

            _loggerFactory = loggerFactory;
            _optionsMonitor = optionsMonitor;
        }

        public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
        {
            ThrowHelper.ThrowIfNull(next);

            return (builder) =>
            {
                // Run other configuration first, we want to decorate.
                next(builder);

                HttpClientFactoryOptions options = _optionsMonitor.Get(builder.Name);
                if (options.SuppressDefaultLogging)
                {
                    return;
                }

                string loggerName = !string.IsNullOrEmpty(builder.Name) ? builder.Name : "Default";

                // We want all of our logging message to show up as-if they are coming from HttpClient,
                // but also to include the name of the client for more fine-grained control.
                ILogger outerLogger = _loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{loggerName}.LogicalHandler");
                ILogger innerLogger = _loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{loggerName}.ClientHandler");

                // The 'scope' handler goes first so it can surround everything.
                builder.AdditionalHandlers.Insert(0, new LoggingScopeHttpMessageHandler(outerLogger, options));

                // We want this handler to be last so we can log details about the request after
                // service discovery and security happen.
                builder.AdditionalHandlers.Add(new LoggingHttpMessageHandler(innerLogger, options));
            };
        }
    }
}
