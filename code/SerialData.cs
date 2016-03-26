// Copyright © Jason Curl 2012-2016
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports
{
    using System;

    /// <summary>
    /// Event related information on DataReceived
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    [Flags]
    public enum SerialData
    {
        // NOTE: Do not change the values of this enum, as it should be the same
        // as Native.Windows.NativeMethods.SerialEventMask.

        /// <summary>
        /// Indicates no data received
        /// </summary>
        NoData = 0,

        /// <summary>
        /// At least a single byte has been received
        /// </summary>
        Chars = 0x0001,

        /// <summary>
        /// The EOF character has been detected
        /// </summary>
        Eof = 0x0002
    }
}
