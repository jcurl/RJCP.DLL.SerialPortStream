namespace RJCP.IO.Ports.Native.Win32
{
    using System;

    internal static partial class Kernel32
    {
        /// <summary>
        /// Specifies possible baud rates in the GetCommProperties method.
        /// </summary>
        [Flags]
        public enum MaxBaud
        {
            /// <summary>
            /// 75 bps.
            /// </summary>
            BAUD_075 = 0x00000001,

            /// <summary>
            /// 110 bps.
            /// </summary>
            BAUD_110 = 0x00000002,

            /// <summary>
            /// 134.5 bps.
            /// </summary>
            BAUD_134_5 = 0x00000004,

            /// <summary>
            /// 150 bps.
            /// </summary>
            BAUD_150 = 0x00000008,

            /// <summary>
            /// 300 bps.
            /// </summary>
            BAUD_300 = 0x00000010,

            /// <summary>
            /// 600 bps.
            /// </summary>
            BAUD_600 = 0x00000020,

            /// <summary>
            /// 1200 bps.
            /// </summary>
            BAUD_1200 = 0x00000040,

            /// <summary>
            /// 1800 bps.
            /// </summary>
            BAUD_1800 = 0x00000080,

            /// <summary>
            /// 2400 bps.
            /// </summary>
            BAUD_2400 = 0x00000100,

            /// <summary>
            /// 4800 bps.
            /// </summary>
            BAUD_4800 = 0x00000200,

            /// <summary>
            /// 7200 bps.
            /// </summary>
            BAUD_7200 = 0x00000400,

            /// <summary>
            /// 9600 bps.
            /// </summary>
            BAUD_9600 = 0x00000800,

            /// <summary>
            /// 14400 bps.
            /// </summary>
            BAUD_14400 = 0x00001000,

            /// <summary>
            /// 19200 bps.
            /// </summary>
            BAUD_19200 = 0x00002000,

            /// <summary>
            /// 38400 bps.
            /// </summary>
            BAUD_38400 = 0x00004000,

            /// <summary>
            /// 56K bps.
            /// </summary>
            BAUD_56K = 0x00008000,

            /// <summary>
            /// 57600 bps.
            /// </summary>
            BAUD_57600 = 0x00040000,

            /// <summary>
            /// 115200 bps.
            /// </summary>
            BAUD_115200 = 0x00020000,

            /// <summary>
            /// 128K bps.
            /// </summary>
            BAUD_128K = 0x00010000,

            /// <summary>
            /// Programmable baud rate.
            /// </summary>
            BAUD_USER = 0x10000000
        }
    }
}
