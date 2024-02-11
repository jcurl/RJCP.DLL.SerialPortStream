﻿namespace RJCP.IO.Ports
{
    using System;

    /// <summary>
    /// Event related data on PinChanged.
    /// </summary>
    [Flags]
    public enum SerialPinChange
    {
        // NOTE: Do not change the values of this enum, as it should be the same
        // as Native.Windows.NativeMethods.SerialEventMask.

        /// <summary>
        /// Indicates no pin change detected.
        /// </summary>
        NoChange = 0,

        /// <summary>
        /// Clear To Send signal has changed.
        /// </summary>
        CtsChanged = 0x08,

        /// <summary>
        /// Data Set Ready signal has changed.
        /// </summary>
        DsrChanged = 0x10,

        /// <summary>
        /// Carrier Detect signal has changed.
        /// </summary>
        CDChanged = 0x20,

        /// <summary>
        /// Break detected.
        /// </summary>
        Break = 0x40,

        /// <summary>
        /// Ring signal has changed.
        /// </summary>
        Ring = 0x100
    }
}
