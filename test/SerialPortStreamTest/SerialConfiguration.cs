using System;
namespace RJCP.IO.Ports.SerialPortStreamTest
{
    using System;
    using System.Configuration;

    public static class SerialConfiguration
    {
        private static object m_SyncLock = new object();
        private static string s_SourcePort = null;
        private static string s_DestPort = null;

        public static string SourcePort
        {
            get
            {
                if (s_SourcePort == null) {
                    lock (m_SyncLock) {
                        if (s_SourcePort == null) {
                            var appSettings = ConfigurationManager.AppSettings;
                            s_SourcePort = appSettings[OSPrefix + "SourcePort"];
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
                            var appSettings = ConfigurationManager.AppSettings;
                            s_DestPort = appSettings[OSPrefix + "DestPort"];
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
                return "";
            }
        }
    }
}
