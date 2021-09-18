// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Native.Win32
{
    using System.Runtime.InteropServices;

    internal static partial class Kernel32
    {
        /// <summary>
        /// Defines the control setting for a serial communications device.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct DCB
        {
            /// <summary>
            /// Length of the structure in bytes.
            /// </summary>
            public int DCBLength;

            /// <summary>
            /// Baud rate at which the communications device operates.
            /// </summary>
            public uint BaudRate;

            /// <summary>
            /// Various flags that define operation.
            /// </summary>
            [MarshalAs(UnmanagedType.U4)]
            public DcbFlags Flags;

            /// <summary>
            /// Not currently used.
            /// </summary>
            private ushort wReserved;

            /// <summary>
            /// Transmit XON threshold.
            /// </summary>
            public ushort XonLim;

            /// <summary>
            /// Transmit XOFF threshold.
            /// </summary>
            public ushort XoffLim;

            /// <summary>
            /// Number of bits in the bytes transmitted and received.
            /// </summary>
            public byte ByteSize;

            /// <summary>
            /// The parity scheme to be used.
            /// </summary>
            public byte Parity;

            /// <summary>
            /// Number of stop bits to use.
            /// </summary>
            public byte StopBits;

            /// <summary>
            /// Value of the XON character for both transmission and reception.
            /// </summary>
            public byte XonChar;

            /// <summary>
            /// Value of the XOFF character for both transmission and reception.
            /// </summary>
            public byte XoffChar;

            /// <summary>
            /// Value of the character to use when replacing bytes with a parity error.
            /// </summary>
            public byte ErrorChar;

            /// <summary>
            /// Character to use to signal end of data.
            /// </summary>
            public byte EofChar;

            /// <summary>
            /// Character to use to signal an event.
            /// </summary>
            public byte EvtChar;

            /// <summary>
            /// Reserved; do not use.
            /// </summary>
            private ushort wReserved1;
        }
    }
}
