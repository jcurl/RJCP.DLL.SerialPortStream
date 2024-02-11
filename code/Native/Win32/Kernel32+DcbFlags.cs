namespace RJCP.IO.Ports.Native.Win32
{
    using System;

    internal static partial class Kernel32
    {
        [Flags]
        public enum DcbFlags
        {
            Binary = 0x0001,
            Parity = 0x0002,
            OutxCtsFlow = 0x0004,
            OutxDsrFlow = 0x0008,
            DtrControlMask = 0x0030,
            DtrControlDisable = 0x0000,
            DtrControlEnable = 0x0010,
            DtrControlHandshake = 0x0020,
            DsrSensitivity = 0x0040,
            TxContinueOnXOff = 0x0080,
            OutX = 0x0100,
            InX = 0x0200,
            ErrorChar = 0x0400,
            Null = 0x0800,
            RtsControlMask = 0x3000,
            RtsControlDisable = 0x0000,
            RtsControlEnable = 0x1000,
            RtsControlHandshake = 0x2000,
            RtsControlToggle = 0x3000,
            AbortOnError = 0x4000
        }
    }
}
