namespace RJCP.IO.Ports.Native.Win32
{
    using System;

    internal static partial class Kernel32
    {
        /// <summary>
        /// Current state of the modem control-register values.
        /// </summary>
        [Flags]
        public enum ModemStat
        {
            /// <summary>
            /// The CTS (clear-to-send) signal is on.
            /// </summary>
            MS_CTS_ON = 0x0010,

            /// <summary>
            /// The DSR (data-set-ready) signal is on.
            /// </summary>
            MS_DSR_ON = 0x0020,

            /// <summary>
            /// The ring indicator signal is on.
            /// </summary>
            MS_RING_ON = 0x0040,

            /// <summary>
            /// The RLSD (receive-line-signal-detect) signal is on.
            /// </summary>
            MS_RLSD_ON = 0x0080
        }
    }
}
