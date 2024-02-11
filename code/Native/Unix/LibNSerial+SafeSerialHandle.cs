namespace RJCP.IO.Ports.Native.Unix
{
    using System;
    using System.Runtime.InteropServices;
#if NETFRAMEWORK
    using System.Runtime.ConstrainedExecution;
#endif

    internal static partial class LibNSerial
    {
        public class SafeSerialHandle : SafeHandle
        {
            public SafeSerialHandle() : base(IntPtr.Zero, true) { }

            public override bool IsInvalid
            {
                get
                {
                    return handle.Equals(IntPtr.Zero);
                }
            }

#if NETFRAMEWORK
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
            protected override bool ReleaseHandle()
            {
                Dll.serial_terminate(handle);
                return true;
            }
        }
    }
}
