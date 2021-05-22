// Copyright © Jason Curl 2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

// This file is only for .NET Core

//#define NETCORE_LOGGER

namespace RJCP.IO.Ports.Trace
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.DependencyInjection;

    internal static class GlobalLogger
    {
#if NETCORE_LOGGER
        private static ServiceCollection m_ServiceCollection;

        public static void Initialize()
        {
            if (m_ServiceCollection == null) {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", false, false)
                    .Build();

                m_ServiceCollection = new ServiceCollection();
                m_ServiceCollection.AddLogging(builder => {
                    builder.AddConfiguration(configuration.GetSection("Logging"));
                    builder.AddConsole();
                });
            }

            // If we don't do this at the beginning of every NUnit test, logging doesn't work. Not clear why, as
            // single-stepping each trace still shows that <c>IsEnabled</c> is <c>true</c>.

            ServiceProvider serviceProvider = m_ServiceCollection.BuildServiceProvider();
            LogSourceFactory.LoggerFactory = serviceProvider.GetService<ILoggerFactory>();
        }
#else
        public static void Initialize()
        {
            if (LogSourceFactory.LoggerFactory == null) {
                LogSourceFactory.LoggerFactory = new Trace.SerialLoggerFactory();
            }
        }
#endif
    }
}
