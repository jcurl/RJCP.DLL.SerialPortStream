﻿namespace RJCP.IO.Ports
{
    using System;

    /// <summary>
    /// Event related information on ErrorReceived.
    /// </summary>
    // Values are obtained from ClearCommErrors() Win32API.
    [Flags]
    public enum SerialError
    {
        // NOTE: Do not change the values of this enum, as it should be the same
        // as Native.Windows.NativeMethods.ComStatErrors.

        /// <summary>
        /// Indicates no error.
        /// </summary>
        NoError = 0,

        /// <summary>
        /// Driver buffer has reached 80% full.
        /// </summary>
        RXOver = 0x0001,

        /// <summary>
        /// Driver has detected an overflow.
        /// </summary>
        Overrun = 0x0002,

        /// <summary>
        /// Parity error detected.
        /// </summary>
        RXParity = 0x0004,

        /// <summary>
        /// Frame error detected.
        /// </summary>
        Frame = 0x0008,

        /// <summary>
        /// Transmit buffer is full.
        /// </summary>
        TXFull = 0x0100
    }
}
