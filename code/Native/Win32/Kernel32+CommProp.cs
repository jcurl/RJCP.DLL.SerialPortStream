namespace RJCP.IO.Ports.Native.Win32
{
    using System.Collections.Specialized;
    using System.Runtime.InteropServices;

    internal static partial class Kernel32
    {
        /// <summary>
        /// Contains information about a communications driver.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct CommProp
        {
            /// <summary>
            /// The size of the entire data packet, regardless of the amount of data requested, in bytes.
            /// </summary>
            public ushort wPacketLength;

            /// <summary>
            /// The version of the structure
            /// </summary>
            public ushort wPacketVersion;

            /// <summary>
            /// A bit mask indicating which services are implemented by this provider. The SP_SERIALCOMM value is always
            /// specified for communications providers, including modem providers.
            /// </summary>
            public uint dwServiceMask;

            /// <summary>
            /// Reserved; do not use.
            /// </summary>
            private uint dwReserved1;

            /// <summary>
            /// The maximum size of the driver's internal output buffer, in bytes. A value of zero indicates that no
            /// maximum value is imposed by the serial provider.
            /// </summary>
            public uint dwMaxTxQueue;

            /// <summary>
            /// The maximum size of the driver's internal input buffer, in bytes. A value of zero indicates that no
            /// maximum value is imposed by the serial provider.
            /// </summary>
            public uint dwMaxRxQueue;

            /// <summary>
            /// The maximum allowable baud rate, in bits per second (bps).
            /// </summary>
            public BitVector32 dwMaxBaud;

            /// <summary>
            /// The communications-provider type.
            /// </summary>
            [MarshalAs(UnmanagedType.U4)]
            public ProvSubType dwProvSubType;

            /// <summary>
            /// A bit mask indicating the capabilities offered by the provider.
            /// </summary>
            public BitVector32 dwProvCapabilities;

            /// <summary>
            /// The communications parameter that can be changed.
            /// </summary>
            public BitVector32 dwSettableParams;

            /// <summary>
            /// The baud rates that can be used.
            /// </summary>
            public BitVector32 dwSettableBaud;

            /// <summary>
            /// The number of data bits that can be set, stop and parity bits in the second half.
            /// </summary>
            public BitVector32 dwSettableDataStopParity;

            /// <summary>
            /// The size of the driver's internal output buffer, in bytes. A value of zero indicates that the value is
            /// unavailable.
            /// </summary>
            public uint dwCurrentTxQueue;

            /// <summary>
            /// The size of the driver's internal input buffer, in bytes. A value of zero indicates that the value is
            /// unavailable.
            /// </summary>
            public uint dwCurrentRxQueue;

            /// <summary>
            /// Any provider-specific data.
            /// </summary>
            /// <remarks>
            /// Applications should ignore this member unless they have detailed information about the format of the
            /// data required by the provider. Set this member to COMMPROP_INITIALIZED before calling the
            /// GetCommProperties function to indicate that the wPacketLength member is already valid.
            /// </remarks>
            public uint dwProvSpec1;

            /// <summary>
            /// Any provider-specific data
            /// </summary>
            /// <remarks>
            /// Applications should ignore this member unless they have detailed information about the format of the
            /// data required by the provider.
            /// </remarks>
            public uint dwProvSpec2;

            /// <summary>
            /// Any provider-specific data
            /// </summary>
            /// <remarks>
            /// Applications should ignore this member unless they have detailed information about the format of the
            /// data required by the provider.
            /// </remarks>
            public char wcProvChar;
        }
    }
}
