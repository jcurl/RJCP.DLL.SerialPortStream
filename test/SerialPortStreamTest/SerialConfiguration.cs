// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports
{
    using System;
    using System.Configuration;

    public static class SerialConfiguration
    {
        private static readonly object m_SyncLock = new object();
        private static string s_SourcePort = null;
        private static string s_DestPort = null;

        private static AppSettingsSection AppSettings
        {
            get
            {
#if NETFRAMEWORK
                return ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).AppSettings;
#else
                // Under .NET Core, the result of
                //  ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath
                // returns
                //  testhost.dll.config
                // where under .NET 4.x it would return
                //  RJCP.SerialPortStreamTest.dll.config
                // where the latter is correct and the actual name of the config file from the build.
                // We need to fix that.
                string assemblyPath = typeof(SerialConfiguration).Assembly.Location;
                return ConfigurationManager.OpenExeConfiguration(assemblyPath).AppSettings;
#endif
            }
        }

        public static string SourcePort
        {
            get
            {
                if (s_SourcePort == null) {
                    lock (m_SyncLock) {
                        if (s_SourcePort == null) {
                            s_SourcePort = AppSettings.Settings[OSPrefix + "SourcePort"].Value;
                        }
                    }
                }
                return s_SourcePort;
            }
        }

        public static string DestPort
        {
            get
            {
                if (s_DestPort == null) {
                    lock (m_SyncLock) {
                        if (s_DestPort == null) {
                            s_DestPort = AppSettings.Settings[OSPrefix + "DestPort"].Value;
                        }
                    }
                }
                return s_DestPort;
            }
        }

        private static string OSPrefix
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                if (p == (int)PlatformID.Win32NT) {
                    return "Win32";
                } else if (p == 4 || p == 8 || p == 128) {
                    return "Linux";
                }
                return string.Empty;
            }
        }
    }
}
