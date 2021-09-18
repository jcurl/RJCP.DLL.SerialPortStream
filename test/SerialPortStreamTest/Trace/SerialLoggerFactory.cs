// Copyright © Jason Curl 2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

// This file is only for .NET Core

namespace RJCP.IO.Ports.Trace
{
    using Microsoft.Extensions.Logging;

    internal sealed class SerialLoggerFactory : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider)
        {
            // There is no provider, as this is a specialized logging interface for the .NET Core logging in this
            // specific unit test environment.
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new SerialLogger();
        }

        public void Dispose()
        {
            // There is nothing to dispose.
        }
    }
}