// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Serial
{
    using System;
#if NETSTANDARD
    using System.Runtime.InteropServices;
#endif

    internal static class Platform
    {
        public static bool IsUnix()
        {
#if NETSTANDARD
            return
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
#else
            int p = (int)Environment.OSVersion.Platform;
            return (p == 4 || p == 8 || p == 128);
#endif
        }

        public static bool IsWinNT()
        {
#if NETSTANDARD
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#else
            int p = (int)Environment.OSVersion.Platform;
            return (p == (int)PlatformID.Win32NT);
#endif
        }
    }
}
