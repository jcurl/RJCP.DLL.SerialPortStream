namespace RJCP.IO.Ports.Native.Win32
{
    using System.Runtime.InteropServices;

    internal static partial class Kernel32
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct COMSTAT
        {
            public ComStatFlags Flags;
            public uint cbInQue;
            public uint cbOutQue;
        }
    }
}
