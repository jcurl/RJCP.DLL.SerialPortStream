namespace RJCP.IO.Ports.Native.Win32
{
    using System;

    internal static partial class Kernel32
    {
        /// <summary>
        /// Communication parameters that can be changed.
        /// </summary>
        [Flags]
        public enum SettableParams
        {
            /// <summary>
            /// Parity.
            /// </summary>
            SP_PARITY = 0x0001,

            /// <summary>
            /// Baud rate.
            /// </summary>
            SP_BAUD = 0x0002,

            /// <summary>
            /// Data bits.
            /// </summary>
            SP_DATABITS = 0x0004,

            /// <summary>
            /// Stop bits.
            /// </summary>
            SP_STOPBITS = 0x0008,

            /// <summary>
            /// Handshaking (flow control).
            /// </summary>
            SP_HANDSHAKING = 0x0010,

            /// <summary>
            /// Parity Checking.
            /// </summary>
            SP_PARITY_CHECK = 0x0020,

            /// <summary>
            /// RLSD (receive-line-signal-detect).
            /// </summary>
            SP_RLSD = 0x0040,
        }
    }
}
