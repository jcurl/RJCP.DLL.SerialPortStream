namespace RJCP.IO.Ports.Native.Win32
{
    using System;

    internal static partial class Kernel32
    {
        /// <summary>
        /// Number of data bits that can be set.
        /// </summary>
        [Flags]
        public enum SettableData
        {
            /// <summary>
            /// 5 data bits.
            /// </summary>
            DATABITS_5 = 0x0001,

            /// <summary>
            /// 6 data bits.
            /// </summary>
            DATABITS_6 = 0x0002,

            /// <summary>
            /// 7 data bits.
            /// </summary>
            DATABITS_7 = 0x0004,

            /// <summary>
            /// 8 data bits.
            /// </summary>
            DATABITS_8 = 0x0008,

            /// <summary>
            /// 16 data bits.
            /// </summary>
            DATABITS_16 = 0x0010,

            /// <summary>
            /// Special wide path through serial hardware lines.
            /// </summary>
            DATABITS_16X = 0x0020
        }
    }
}
