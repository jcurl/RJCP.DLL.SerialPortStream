namespace RJCP.IO.Ports.Native.Win32
{
    using System;

    internal static partial class Kernel32
    {
        /// <summary>
        /// The stop bit and parity settings.
        /// </summary>
        [Flags]
        public enum SettableStopParity
        {
            /// <summary>
            /// 1 stop bit.
            /// </summary>
            STOPBITS_10 = 0x00010000,

            /// <summary>
            /// 1.5 stop bits.
            /// </summary>
            STOPBITS_15 = 0x00020000,

            /// <summary>
            /// 2 stop bits.
            /// </summary>
            STOPBITS_20 = 0x00040000,

            /// <summary>
            /// No parity.
            /// </summary>
            PARITY_NONE = 0x01000000,

            /// <summary>
            /// Odd parity.
            /// </summary>
            PARITY_ODD = 0x02000000,

            /// <summary>
            /// Even parity.
            /// </summary>
            PARITY_EVEN = 0x04000000,

            /// <summary>
            /// Mark parity.
            /// </summary>
            PARITY_MARK = 0x08000000,

            /// <summary>
            /// Space parity.
            /// </summary>
            PARITY_SPACE = 0x10000000
        }
    }
}
