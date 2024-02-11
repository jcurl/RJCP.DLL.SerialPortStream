namespace RJCP.IO.Ports.Native.Win32
{
    using System;

    internal static partial class Kernel32
    {
        [Flags]
        public enum PurgeFlags
        {
            PURGE_TXABORT = 0x0001,
            PURGE_RXABORT = 0x0002,
            PURGE_TXCLEAR = 0x0004,
            PURGE_RXCLEAR = 0x0008
        }
    }
}
