// Copyright © Jason Curl 2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

// This file is only for .NET Core

namespace RJCP.IO.Ports.Trace
{
    using Microsoft.Extensions.Logging;
    using RJCP.CodeQuality.NUnitExtensions.Trace;
    using RJCP.Diagnostics.Trace;

    internal static class GlobalLogger
    {
        static GlobalLogger()
        {
            ILoggerFactory factory = LoggerFactory.Create(builder => {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("RJCP", LogLevel.Debug)
                    .AddNUnitLogger();
            });
            LogSource.SetLoggerFactory(factory);
        }

        // Just calling this method will result in the static constructor being executed.
        public static void Initialize()
        {
            /* No code necessary. Accessing this method will execute static constructor automatically */
        }
    }
}
