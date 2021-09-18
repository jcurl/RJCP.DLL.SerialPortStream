// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Native.Win32
{
    internal static partial class Kernel32
    {
        /// <summary>
        /// Communications Provider Type.
        /// </summary>
        public enum ProvSubType
        {
            /// <summary>
            /// Unspecified.
            /// </summary>
            PST_UNSPECIFIED = 0x00000000,

            /// <summary>
            /// RS-232 serial port.
            /// </summary>
            PST_RS232 = 0x00000001,

            /// <summary>
            /// Parallel port.
            /// </summary>
            PST_PARALLELPORT = 0x00000002,

            /// <summary>
            /// RS-422 port.
            /// </summary>
            PST_RS422 = 0x00000003,

            /// <summary>
            /// RS-423 port.
            /// </summary>
            PST_RS423 = 0x00000004,

            /// <summary>
            /// RS-449 port.
            /// </summary>
            PST_RS449 = 0x00000005,

            /// <summary>
            /// Modem Device.
            /// </summary>
            PST_MODEM = 0x00000006,

            /// <summary>
            /// FAX Device.
            /// </summary>
            PST_FAX = 0x00000021,

            /// <summary>
            /// Scanner device.
            /// </summary>
            PST_SCANNER = 0x00000022,

            /// <summary>
            /// Unspecified network bridge.
            /// </summary>
            PST_NETWORK_BRIDGE = 0x00000100,

            /// <summary>
            /// LAT protocol.
            /// </summary>
            PST_LAT = 0x00000101,

            /// <summary>
            /// TCP/IP Telnet Protocol.
            /// </summary>
            PST_TCPIP_TELNET = 0x00000102,

            /// <summary>
            /// X.25 standards.
            /// </summary>
            PST_X25 = 0x00000103
        }
    }
}
