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
        private static readonly object s_LoggerFactoryLock = new object();
        private static ILoggerFactory s_LoggerFactory;

        private static ILoggerFactory GetLoggerFactory()
        {
            if (s_LoggerFactory == null) {
                lock (s_LoggerFactoryLock) {
                    if (s_LoggerFactory == null) {
                        s_LoggerFactory = LoggerFactory.Create(builder => {
                            builder
                                .AddFilter("Microsoft", LogLevel.Warning)
                                .AddFilter("System", LogLevel.Warning)
                                .AddFilter("RJCP", LogLevel.Debug)
                                .AddNUnitLogger();
                        });
                    }
                }
            }
            return s_LoggerFactory;
        }

        public static void Initialize()
        {
            LogSource.SetLoggerFactory(GetLoggerFactory());
        }
    }
}
