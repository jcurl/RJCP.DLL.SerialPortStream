namespace RJCP.IO.Ports.Native.Win32
{
    using System;

    internal static partial class Kernel32
    {
        [Flags]
        public enum SerialEventMask
        {
            /// <summary>
            /// A character was received and placed in the input buffer.
            /// </summary>
            EV_RXCHAR = 0x0001,

            /// <summary>
            /// The event character was received and placed in the input buffer. The event character is specified in the
            /// device's DCB structure, which is applied to a serial port by using the SetCommState function.
            /// </summary>
            EV_RXFLAG = 0x0002,

            /// <summary>
            /// The last character in the output buffer was sent.
            /// </summary>
            EV_TXEMPTY = 0x0004,

            /// <summary>
            /// The CTS (clear-to-send) signal changed state.
            /// </summary>
            EV_CTS = 0x0008,

            /// <summary>
            /// The DSR (data-set-ready) signal changed state.
            /// </summary>
            EV_DSR = 0x0010,

            /// <summary>
            /// The RLSD (receive-line-signal-detect) signal changed state.
            /// </summary>
            EV_RLSD = 0x0020,

            /// <summary>
            /// A break was detected on input.
            /// </summary>
            EV_BREAK = 0x0040,

            /// <summary>
            /// A line-status error occurred. Line-status errors are CE_FRAME, CE_OVERRUN, and CE_RXPARITY.
            /// </summary>
            EV_ERR = 0x0080,

            /// <summary>
            /// A ring indicator was detected.
            /// </summary>
            EV_RING = 0x0100,

            /// <summary>
            /// A printer error occurred.
            /// </summary>
            EV_PERR = 0x0200,

            /// <summary>
            /// The receive buffer is 80 percent full.
            /// </summary>
            EV_RX80FULL = 0x0400,

            /// <summary>
            /// An event of the first provider-specific type occurred.
            /// </summary>
            EV_EVENT1 = 0x0800,

            /// <summary>
            /// An event of the second provider-specific type occurred.
            /// </summary>
            EV_EVENT2 = 0x1000
        }
    }
}
