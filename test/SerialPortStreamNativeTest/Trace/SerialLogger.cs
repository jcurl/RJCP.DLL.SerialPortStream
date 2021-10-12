// Copyright © Jason Curl 2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

// This file is only for .NET Core

namespace RJCP.IO.Ports.Trace
{
    using System;
    using Microsoft.Extensions.Logging;

    internal sealed class SerialLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Console.WriteLine(formatter(state, exception));
        }
    }
}