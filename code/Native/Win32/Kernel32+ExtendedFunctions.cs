// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Native.Win32
{
    internal static partial class Kernel32
    {
        /// <summary>
        /// The extended function to be performed.
        /// </summary>
        public enum ExtendedFunctions
        {
            /// <summary>
            /// Causes transmission to act as if an XOFF character has been received.
            /// </summary>
            SETXOFF = 1,

            /// <summary>
            /// Causes transmission to act as if an XON character has been received.
            /// </summary>
            SETXON = 2,

            /// <summary>
            /// Sends the RTS (request-to-send) signal.
            /// </summary>
            SETRTS = 3,

            /// <summary>
            /// Clears the RTS (request-to-send) signal.
            /// </summary>
            CLRRTS = 4,

            /// <summary>
            /// Sends the DTR (data-terminal-ready) signal.
            /// </summary>
            SETDTR = 5,

            /// <summary>
            /// Clears the DTR (data-terminal-ready) signal.
            /// </summary>
            CLRDTR = 6,

            /// <summary>
            /// Suspends character transmission and places the transmission line in a break state until the
            /// ClearCommBreak function is called (or EscapeCommFunction is called with the CLRBREAK extended function
            /// code). The SETBREAK extended function code is identical to the SetCommBreak function. Note that this
            /// extended function does not flush data that has not been transmitted.
            /// </summary>
            SETBREAK = 8,

            /// <summary>
            /// Restores character transmission and places the transmission line in a non-break state. The CLRBREAK
            /// extended function code is identical to the ClearCommBreak function
            /// </summary>
            CLRBREAK = 9
        }
    }
}
